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
            // Builds a context that has all possible accelerators.
            //using Context context = Context.CreateDefault();

            // Builds a context that only has CPU accelerators.
            //using Context context = Context.Create(builder => builder.CPU());

            // Builds a context that only has Cuda accelerators.
            //using Context context = Context.Create(builder => builder.Cuda());

            // Builds a context that only has OpenCL accelerators.
            //using Context context = Context.Create(builder => builder.OpenCL());

            // Builds a context with only OpenCL and Cuda acclerators.
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