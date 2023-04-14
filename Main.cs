using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.IO;

namespace ReallyBigNumbersGPU
{
    public class Test
    {
        public static void Main(string[] args)
        {

            Context context = Context.CreateDefault();

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
        }

        private static string GetInfoString(Accelerator a)
        {
            StringWriter infoString = new StringWriter();
            a.PrintInformation(infoString);
            return infoString.ToString();
        }

    }
}