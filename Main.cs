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
            Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
                                      .CreateAccelerator(context);

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