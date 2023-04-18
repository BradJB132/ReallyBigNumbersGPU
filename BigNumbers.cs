using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.IR.Types;
using ILGPU.Algorithms.Random;
using System.Runtime.CompilerServices;
using ILGPU.Algorithms;

namespace ReallyBigNumbersGPU
{
    internal class BigNumbers
    {
        private Accelerator accelerator;
        private int MAX_VAR_SIZE = 4096;

        public BigNumbers(Accelerator accelerator)
        {
            this.accelerator = accelerator;
        }

        public int GenerateBigNumber(String fileName, long sizeKB)
        {

            if(File.Exists(fileName))
            {
                return 0;
            }
            StreamWriter streamWriter = File.AppendText(fileName);
            FileInfo fileInfo = new FileInfo(fileName);

            var random = new Random();
            using var rng = RNG.Create<XorShift128Plus>(accelerator, random);

            using var buffer = accelerator.Allocate1D<long>(MAX_VAR_SIZE);
            long sizeGuess = 0;
            while (sizeGuess < sizeKB * 1000000)
            {
                rng.FillUniform(accelerator.DefaultStream, buffer.View);
                var randValues = buffer.GetAsArray1D();
                foreach( var randValue in randValues) { streamWriter.Write(Math.Abs(randValue).ToString()); sizeGuess += 18; }
            }
            buffer.Dispose();
            rng.Dispose();
            streamWriter.Close();
            return 1;

        }

        public int AddBigNumber(String inFile1, String inFile2, String outFile)
        {

            static void KernelAddNumber(Index1D index, ArrayView1D<long, Stride1D.Dense> inData1, ArrayView1D<long, Stride1D.Dense> inData2, ArrayView1D<long, Stride1D.Dense> carryData, ArrayView1D<long, Stride1D.Dense> outData)
            {
                outData[index] = inData1[index] + inData2[index];
                if (outData[index].ToString().Length > 16 || outData[index].ToString().Length > Math.Max(inData1[index], inData2[index]).ToString().Length)
                {
                    outData[index] = (long)(outData[index] - Math.Pow(10, outData[index].ToString().Length - 1));
                    carryData[index] = 1;
                }
                else carryData[index] = 0;
            }

            static void KernelRecurseCarry(Index1D index, ArrayView1D<long, Stride1D.Dense> numbers, ArrayView1D<long, Stride1D.Dense> carry)
            {
                numbers[index] += carry[index];
                carry[index] = 0;
                if((numbers[index].ToString().Length > 16))
                    carry[index] = 1;
            }

            if (File.Exists(outFile))
            {
                return 0;
            }

            StreamWriter streamWriter = new StreamWriter(outFile);

            FileInfo info1 = new FileInfo(inFile1);
            FileInfo info2 = new FileInfo(inFile2);

            var biggerTemp = info1.Length >= info2.Length ? inFile1 : inFile2;
            var smallerTemp = info1.Length < info2.Length ? inFile1 : inFile2;

            StreamReader bigReader = new StreamReader(biggerTemp);
            StreamReader smallReader = new StreamReader(smallerTemp);

            FileInfo bigInfo = new FileInfo(biggerTemp);
            FileInfo smallInfo = new FileInfo(smallerTemp);

            MemoryBuffer1D<long, Stride1D.Dense> bigNumber;
            MemoryBuffer1D<long, Stride1D.Dense> smallNumber;
            MemoryBuffer1D<long, Stride1D.Dense> outNumber;
            MemoryBuffer1D<long, Stride1D.Dense> carryNum;
            MemoryBuffer1D<long, Stride1D.Dense> carryTemp;

            Action<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>> addNumber = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>>(KernelAddNumber);
            Action<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>> recurseCarry = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<long, Stride1D.Dense>>(KernelRecurseCarry);

            long[] carryShift;
            long pos = 0;
            //Add numbers together with carries
            while(!bigReader.EndOfStream || !smallReader.EndOfStream)
            {
                //allocate new gpu arrays
                bigNumber = accelerator.Allocate1D<long>(convertReadToLong(bigReader, pos));
                smallNumber = accelerator.Allocate1D<long>(convertReadToLong(smallReader, pos++));
                outNumber = accelerator.Allocate1D<long>(MAX_VAR_SIZE);
                carryNum = accelerator.Allocate1D<long>(MAX_VAR_SIZE);
                //add numbers together
                addNumber((int)bigNumber.Length, bigNumber, smallNumber, outNumber, carryNum);
                //shift carry array to align with correct numbers
                carryShift = carryNum.GetAsArray1D();
                carryShift = rightShift(carryShift);
                carryNum.CopyFromCPU<long>(carryShift);
                //begin solving carries
                while (carryShift.Contains(1))
                {
                    recurseCarry((int)outNumber.Length, outNumber, carryNum);

                }
            }
            return 1;
        } 

        private long[] rightShift(long[] arr)
        {
            long[] temp = new long[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                temp[(i + 1) % temp.Length] = arr[i];
            }
            return temp;
        }

        private long[] convertReadToLong(StreamReader streamReader, long position)
        {
            long[] ret = new long[MAX_VAR_SIZE];
            char[] test = new char[16];
            for (long i = 0; i < MAX_VAR_SIZE; i++)
            {
                streamReader.BaseStream.Seek(-16 + (i * -16), SeekOrigin.End);
                streamReader.ReadBlock(test);
                var testTemp = new string(test); 
                long number = Convert.ToInt64(testTemp);
                ret[i] = number;
            }
            return ret;
        }

    }
}
