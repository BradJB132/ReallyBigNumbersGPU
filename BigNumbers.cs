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
            streamWriter.Close();
            return 1;

        }

        public int AddBigNumber(String inFile1, String inFile2, String outFile)
        {

            static void KernelSuperMath(Index1D index, ArrayView1D<long, Stride1D.Dense> inData1, ArrayView1D<long, Stride1D.Dense> inData2, ArrayView1D<long, Stride1D.Dense> carryData, ArrayView1D<long, Stride1D.Dense> outData)
            {
                outData[index] = inData1[index] + inData2[index] + carryData[index];
                if (outData[index].ToString().Length > Math.Max(inData1[index], inData2[index]).ToString().Length)
                    carryData[index] = 1;
                else carryData[index] = 0;
            }

            if (File.Exists(outFile))
            {
                return 0;
            }
            StreamReader inReader1 = new StreamReader(inFile1);
            StreamReader inReader2 = new StreamReader(inFile2);
            StreamWriter streamWriter = new StreamWriter(outFile);

            using var buffer = accelerator.Allocate1D<long>(4096);


        } 

    }
}
