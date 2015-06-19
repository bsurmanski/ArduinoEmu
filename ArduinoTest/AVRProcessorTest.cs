using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Arduino;

namespace ArduinoTest
{
    [TestClass]
    public class AVRProcessorTest
    {
        AVRProcessor proc;

        byte[] hexBlink;
        string blink =  ":100010000C9479000C9479000C9479000C9479007C" +
                        ":100020000C9479000C9479000C9479000C9479006C" +
                        ":100030000C9479000C9479000C9479000C9479005C" +
                        ":100040000C949A000C9479000C9479000C9479002B" +
                        ":100050000C9479000C9479000C9479000C9479003C" +
                        ":100060000C9479000C947900000000080002010053" +
                        ":100070000003040700000000000000000102040863" +
                        ":100080001020408001020408102001020408102002" +
                        ":10009000040404040404040402020202020203032E" +
                        ":1000A0000303030300000000250028002B000000CC" +
                        ":1000B0000000240027002A0011241FBECFEFD8E043" +
                        ":1000C000DEBFCDBF11E0A0E0B1E0EAE2F4E002C0A3" +
                        ":1000D00005900D92A230B107D9F721E0A2E0B1E07E" +
                        ":1000E00001C01D92AB30B207E1F70E9403020C94ED" +
                        ":1000F00013020C94000061E0809100010C949301C4" +
                        ":10010000CF93DF93C0E0D1E061E088810E94CC0111" +
                        ":1001100068EE73E080E090E00E94070160E0888173" +
                        ":100120000E94CC0168EE73E080E090E0DF91CF9117" +
                        ":100130000C9407011F920F920FB60F9211242F9368" +
                        ":100140003F938F939F93AF93BF93809103019091BF" +
                        ":100150000401A0910501B09106013091020123E054" +
                        ":10016000230F2D3720F40196A11DB11D05C026E8EF" +
                        ":10017000230F0296A11DB11D20930201809303015C" +
                        ":1001800090930401A0930501B093060180910701AB" +
                        ":1001900090910801A0910901B0910A010196A11D59" +
                        ":1001A000B11D8093070190930801A0930901B093BA" +
                        ":1001B0000A01BF91AF919F918F913F912F910F9025" +
                        ":1001C0000FBE0F901F9018953FB7F89480910701CC" +
                        ":1001D00090910801A0910901B0910A0126B5A89B50" +
                        ":1001E00005C02F3F19F00196A11DB11D3FBF662725" +
                        ":1001F000782F892F9A2F620F711D811D911D42E06A" +
                        ":10020000660F771F881F991F4A95D1F70895CF92DF" +
                        ":10021000DF92EF92FF92CF93DF936B017C010E94FC" +
                        ":10022000E400EB01C114D104E104F10489F00E945F" +
                        ":1002300012020E94E4006C1B7D0B683E734090F339" +
                        ":1002400081E0C81AD108E108F108C851DC4FEACFB3" +
                        ":10025000DF91CF91FF90EF90DF90CF900895789449" +
                        ":1002600084B5826084BD84B5816084BD85B58260BB" +
                        ":1002700085BD85B5816085BDEEE6F0E08081816059" +
                        ":100280008083E1E8F0E01082808182608083808159" +
                        ":1002900081608083E0E8F0E0808181608083E1EB31" +
                        ":1002A000F0E0808184608083E0EBF0E08081816019" +
                        ":1002B0008083EAE7F0E080818460808380818260CF" +
                        ":1002C00080838081816080838081806880831092B8" +
                        ":1002D000C1000895833081F028F4813099F0823094" +
                        ":1002E000A1F008958730A9F08830B9F08430D1F4B6" +
                        ":1002F000809180008F7D03C0809180008F778093F4" +
                        ":100300008000089584B58F7702C084B58F7D84BD49" +
                        ":1003100008958091B0008F7703C08091B0008F7DE9" +
                        ":100320008093B0000895CF93DF9390E0FC01E458F0" +
                        ":10033000FF4F2491FC01E057FF4F8491882349F13E" +
                        ":1003400090E0880F991FFC01E255FF4FA591B491F1" +
                        ":100350008C559F4FFC01C591D4919FB7611108C086" +
                        ":10036000F8948C91209582238C93888182230AC0F3" +
                        ":10037000623051F4F8948C91322F309583238C9312" +
                        ":100380008881822B888304C0F8948C91822B8C9373" +
                        ":100390009FBFDF91CF9108950F931F93CF93DF936A" +
                        ":1003A0001F92CDB7DEB7282F30E0F901E859FF4F93" +
                        ":1003B0008491F901E458FF4F1491F901E057FF4F80" +
                        ":1003C00004910023C9F0882321F069830E946A0107" +
                        ":1003D0006981E02FF0E0EE0FFF1FEC55FF4FA59174" +
                        ":1003E000B4919FB7F8948C91611103C0109581234B" +
                        ":1003F00001C0812B8C939FBF0F90DF91CF911F91F4" +
                        ":100400000F91089508950E942F010E9402020E94F8" +
                        ":100410007B00C0E0D0E00E9480002097E1F30E94C2" +
                        ":0A0420000000F9CF0895F894FFCF13" +
                        ":02042A000D00C3" +
                        ":00000001FF";


        string initASM = "11241fbecfefd8e0debfcdbf"; // sets up SP and status register
        string stackTestASM = "";

        public AVRProcessorTest() {
            hexBlink = System.Text.Encoding.ASCII.GetBytes(blink);

            Memory instMem = new Memory(0x8000);
            Memory dataMem = new Memory(0x900);
            proc = new AVRProcessor(instMem, dataMem);
        }

        private void initProcessor() {
            Stream initstream = new HexadecimalStream(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(initASM)));

            proc.Reset();
            proc.getDataMemory().Zero();
            proc.getProgramMemory().Fill(initstream);

            for (int i = 0; i < 6; i++) {
                AVRInstruction inst = proc.decode();
                Console.WriteLine(inst.ToString());
                proc.execute(inst);
            }
        }

        [TestMethod]
        public void Initialize() {
            initProcessor();

            string expected = "00000000000000000000000000000000" +
                              "000000000000000000000000FF080000" + // registers
                              "00000000000000000000000000000000" +
                              "00000000000000000000000000000000" +
                              "00000000000000000000000000000000" +
                              "00000000000000000000000000FF0800" + // stack pointer
                              "00000000000000000000000000000000";

            Stream expectedStream = new HexadecimalStream(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(expected)));

            Console.WriteLine(proc.getDataMemory().ToString());
            for (uint i = 0; i < 0x60; i++) {
                Console.WriteLine(i);
                Assert.AreEqual(expectedStream.ReadByte(), proc.getDataMemory().readByte(i));
            }
        } 

        [TestMethod]
        public void Stack() {
            initProcessor();
        }

        [TestMethod]
        public void Execute() {
            proc.Reset();
            proc.getDataMemory().Zero();
            proc.getProgramMemory().Fill(hexBlink);

            try {
                for (int i = 0; i < 100; i++) {
                    AVRInstruction inst = proc.decode();
                    Console.WriteLine(proc.getPC().value().ToString("X2") + ": " + inst.ToString());
                    proc.execute(inst);
                }
            } catch (Exception e) {
                Console.WriteLine(proc.getDataMemory().ToString());
            }
        }

        [TestMethod]
        public void SignExtend() {
            int a = 0x0fff; // 12 bit (-1)
            int b = (int) (((uint) a) << 20) >> 20;
            Assert.AreEqual(b, -1);
        }
    }
}
