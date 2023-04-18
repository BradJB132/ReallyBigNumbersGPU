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

            using var buffer = accelerator.Allocate1D<long>(4096);
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

            static void KernelAddNumber(Index1D index, ArrayView<long> inData1, ArrayView<long> inData2, ArrayView<long> carryData, ArrayView<long> outData)
            {
                outData[index] = inData1[index] + inData2[index];
                if (outData[index].ToString().Length > 16 || outData[index].ToString().Length > Math.Max(inData1[index], inData2[index]).ToString().Length)
                {
                    outData[index] = (long)(outData[index] - Math.Pow(10, outData[index].ToString().Length - 1));
                    carryData[index + 1] = 1;
                }
                else carryData[index + 1] = 0;
            }

            static void KernelRecurseCarry(Index1D index, ArrayView<long> numbers, ArrayView<long> carry, ArrayView<long> newCarry)
            {
                numbers[index] += carry[index];
                if((numbers[index].ToString().Length > 16))
                    newCarry[index + 1] = 1;
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

            Action<Index1D, ArrayView<long>, ArrayView<long>, ArrayView<long>, ArrayView<long>> addNumber = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<long>, ArrayView<long>, ArrayView<long>, ArrayView<long>>(KernelAddNumber);
            Action<Index1D, ArrayView<long>, ArrayView<long>, ArrayView<long>> recurseCarry = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<long>, ArrayView<long>, ArrayView<long>>(KernelRecurseCarry);
            //Add numbers together with carries
            while(!bigReader.EndOfStream || !smallReader.EndOfStream)
            {
             

            }
            return 1;
        } 

        public long[] convertReadToLong(StreamReader streamReader, long position)
        {
            long[] ret = new long[4096];
            char[] test = new char[16];
            for (long i = 0; i < 4096; i++)
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
