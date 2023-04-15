using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using ILGPU.Algorithms;
using System;
using System.IO;
using System.Diagnostics;

namespace ReallyBigNumbersGPU
{
    public class Test
    {
        public static void Main(string[] args)
        {

            Context context = Context.Create(builder => builder.Default().EnableAlgorithms());

            Accelerator accelerator;
            try
            {
                accelerator = context.GetPreferredDevice(preferCPU: false)
                                          .CreateAccelerator(context);
            }
            catch (ILGPU.Runtime.OpenCL.CLException)
            {
                accelerator = context.GetCudaDevice(0).CreateAccelerator(context);
            }
            catch (ILGPU.Runtime.Cuda.CudaException)
            {
                accelerator = context.GetCPUDevice(0).CreateCPUAccelerator(context);
            }
            Console.WriteLine(GetInfoString(accelerator));

            Stopwatch stopwatch = Stopwatch.StartNew();
            BigNumbers bigNumbers = new BigNumbers(accelerator);
            bigNumbers.GenerateBigNumber("C:\\Users\\Brad\\Desktop\\realtest.txt", 1500);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine("Runtime= " + ts.Seconds + "." + ts.Milliseconds);
        }

        
        private static string GetInfoString(Accelerator a)
        {
            StringWriter infoString = new StringWriter();
            a.PrintInformation(infoString);
            return infoString.ToString();
        }

    }
}