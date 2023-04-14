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

namespace ReallyBigNumbersGPU
{
    internal class BigNumbers
    {
        private Accelerator accelerator;

        public BigNumbers(Accelerator accelerator)
        {
            this.accelerator = accelerator;
        }

        private

        public int generateBigNumber(String fileName, long sizeKB)
        {
            if(File.Exists(fileName))
            {
                return 0;
            }
            StreamWriter streamWriter = File.AppendText(fileName);
            FileInfo fileInfo = new FileInfo(fileName);

            var random = new Random();
            using var rng = RNG.Create<XorShift128Plus>(accelerator, random);

            using var buffer = accelerator.Allocate1D<long>(16);
            while (fileInfo.Length < sizeKB * 1024)
            {
                rng.FillUniform(accelerator.DefaultStream, buffer.View);
                var randValues = buffer.GetAsArray1D();
                foreach( var randValue in randValues) { streamWriter.WriteLine(randValue.ToString()); }
            }

        }

    }
}
