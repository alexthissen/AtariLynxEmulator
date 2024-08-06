using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors.Tests.Suites
{
    [TestClass]
    public class FunctionalTests
    {
        [TestMethod]
        [DeploymentItem("Binaries/6502_functional_test.bin")]
        public void Functional6502Test()
        {
            byte[] binary = File.ReadAllBytes("6502_functional_test.bin");
            Ram64KBMemoryStub ram = new Ram64KBMemoryStub();
            Cmos65SC02 cpu = new Cmos65SC02(ram, new Clock());
            byte[] memory = ram.GetDirectAccess();
            Array.Copy(binary, 0, memory, 0x0000, binary.Length);
            cpu.Reset();
            cpu.PC = 0x0400;

            int instructions = 30646176;
            while (instructions-- > 0) 
                cpu.Execute(1);

            Assert.AreEqual(cpu.PC, 0x3469);
            Assert.AreEqual(cpu.A, 0xf0, "Testing complete opcode incorrect");
        }
    }
}
