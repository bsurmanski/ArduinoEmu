using System;
using System.Collections;

namespace Arduino
{
    public class AVRProcessor : Processor
    {
        Memory dataMemory;
        Memory progMemory;

        MemoryAddress pc; // 12 bits
        bool isAVRTiny; // AVR Tiny devices have alternate instructions for load/store

        public AVRProcessor(Memory progMem, Memory dataMem) {
            progMemory = progMem;
            dataMemory = dataMem;
            isAVRTiny = false;
            pc = new MemoryAddress(progMemory, 0);
        }

        public void Reset() {
            pc = new MemoryAddress(progMemory, 0);
        }

        public Memory getMemory() {
            return dataMemory;
        }

        public Memory getDataMemory() {
            return dataMemory;
        }

        public Memory getProgramMemory() {
            return progMemory;
        }

        public MemoryAddress getPC() {
            return pc;
        }

        public void setPC(MemoryAddress addr) {
            pc = addr;
        }

        public ushort readStackPointer() {
            return dataMemory.readWord(0x5D);
        }

        public void writeStackPointer(ushort value) {
            dataMemory.writeWord(0x5D, value);
        }

        public void pushStackWord(ushort val) {
            ushort sp = readStackPointer();
            dataMemory.writeWord(sp, val);
            writeStackPointer((ushort) (sp - 2));
        }

        public ushort popStackWord() {
            ushort sp = (ushort) (readStackPointer() + 2);
            ushort val = dataMemory.readWord(sp);
            writeStackPointer(sp);
            return val;
        }

        public void pushStack(byte val) {
            ushort sp = readStackPointer();
            dataMemory.writeByte(sp, val);
            writeStackPointer((ushort)(sp - 1));
        }

        public byte popStack() {
            ushort sp = (ushort) (readStackPointer() + 1);
            byte val = dataMemory.readByte(sp);
            writeStackPointer(sp);
            return val;
        }

        public byte readRegister(uint i) {
            if (i >= 32) throw new Exception("register id must be between 0-32");
            return dataMemory.readByte(i); // registers are memory mapped from 0x00 to 0x20
        }

        public void writeRegister(uint i, byte b) {
            if (i >= 32) throw new Exception("register id must be between 0-32");
            dataMemory.writeByte(i, b);
        }

        public ushort readRegisterPair(uint i) {
            if (i > 32) throw new Exception("register id must be betwee 0-32");
            return dataMemory.readWord(i);
        }

        public void writeRegisterPair(uint i, ushort val) {
            if (i >= 32) throw new Exception("register id must be between 0-32");
            dataMemory.writeWord(i, val);
        }

        public ushort readRegisterX() {
            return dataMemory.readWord(26);
        }

        public ushort readRegisterY() {
            return dataMemory.readWord(28);
        }

        public ushort readRegisterZ() {
            return dataMemory.readWord(30);
        }

        public void writeRegisterX(ushort val) {
            dataMemory.writeWord(26, val);
        }

        public void writeRegisterY(ushort val) {
            dataMemory.writeWord(28, val);
        }

        public void writeRegisterZ(ushort val) {
            dataMemory.writeWord(30, val);
        }

        public byte readStatus() {
            return dataMemory.readByte(0x5f); // status register is memory mapped
        }

        public void writeStatus(byte b) {
            dataMemory.writeByte(0x5f, b);
        }

        const ushort b0 = 0x01;
        const ushort b1 = 0x01 << 1;
        const ushort b2 = 0x01 << 2;
        const ushort b3 = 0x01 << 3;
        const ushort b4 = 0x01 << 4;
        const ushort b5 = 0x01 << 5;
        const ushort b6 = 0x01 << 6;
        const ushort b7 = 0x01 << 7;
        const ushort b8 = 0x01 << 8;
        const ushort b9 = 0x01 << 9;
        const ushort b10 = 0x01 << 10;
        const ushort b11 = 0x01 << 11;
        const ushort b12 = 0x01 << 12;
        const ushort b13 = 0x01 << 13;
        const ushort b14 = 0x01 << 14;
        const ushort b15 = 0x01 << 15;

        const byte status_C = 0x01 << 0;
        const byte status_Z = 0x01 << 1;
        const byte status_N = 0x01 << 2;
        const byte status_V = 0x01 << 3;
        const byte status_S = 0x01 << 4;
        const byte status_H = 0x01 << 5;
        const byte status_T = 0x01 << 6;
        const byte status_I = 0x01 << 7;

        static bool isBitSet(ushort op, ushort bit) {
            return (op & bit) != 0;
        }

        AVRInstruction decode_0000(ushort op) {
            if (!isBitSet(op, b11) && !isBitSet(op, b10)) {
                if (isBitSet(op,b9)) {
                    return decode_0000_001(op); // *MUL* type instructions
                } else {
                    return new AVRInstruction(AVRInstruction.Mnemonic.MOVW, pc);
                }
            } else if (!isBitSet(op, b11) && isBitSet(op, b10)) {
                //CPC
                return new AVRInstruction(AVRInstruction.Mnemonic.CPC, pc);
            } else if (isBitSet(op, b11) && !isBitSet(op, b10)) {
                //SBC
                return new AVRInstruction(AVRInstruction.Mnemonic.SBC, pc);
            } else if (isBitSet(op, b11) && isBitSet(op, b10)) {
                //ADD
                return new AVRInstruction(AVRInstruction.Mnemonic.ADD, pc);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        // *MUL
        AVRInstruction decode_0000_001(ushort op) {
            if (isBitSet(op, b8)) {
                return new AVRInstruction(AVRInstruction.Mnemonic.MULS, pc);
            } else {
                if (!isBitSet(op, b7) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.MULSU, pc);
                } else if (!isBitSet(op, b7) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMUL, pc);
                } else if (isBitSet(op, b7) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMULS, pc);
                } else if (isBitSet(op, b7) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMULSU, pc);
                }
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_0001(ushort op) {
            ushort op10to12 = (ushort)((op & (b10 | b11)));
            if (op10to12 == 0x0000) {
                //CPSE
                return new AVRInstruction(AVRInstruction.Mnemonic.CPSE, pc);
            } else if (op10to12 == 0x0400) {
                //CP
                return new AVRInstruction(AVRInstruction.Mnemonic.CP, pc);
            } else if (op10to12 == 0x0800) {
                //SUB
                return new AVRInstruction(AVRInstruction.Mnemonic.SUB, pc);
            } else if (op10to12 == 0x0c00) {
                //ADC
                return new AVRInstruction(AVRInstruction.Mnemonic.ADC, pc);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_0010(ushort op) {
            ushort op10to12 = (ushort)((op & (b10 | b11)));
            if (op10to12 == 0x0000) {
                //AND
                return new AVRInstruction(AVRInstruction.Mnemonic.AND, pc);
            } else if (op10to12 == 0x0400) {
                //EOR
                return new AVRInstruction(AVRInstruction.Mnemonic.EOR, pc);
            } else if (op10to12 == 0x0800) {
                //MOV
                return new AVRInstruction(AVRInstruction.Mnemonic.MOV, pc);
            } else if (op10to12 == 0x0c00) {
                //OR
                return new AVRInstruction(AVRInstruction.Mnemonic.OR, pc);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_10x0(ushort op) {
            if (isAVRTiny) {
                //TODO: LDS16/STS16
            } else {
                if (!isBitSet(op, b9) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.LDDZ, pc);
                } else if (!isBitSet(op, b9) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.LDDY, pc);
                } else if (isBitSet(op, b9) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.STDZ, pc);
                } else if (isBitSet(op, b9) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.STDY, pc);
                } 
            }
            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001(ushort op) {
            if ((op & (b11 | b10 | b9)) == 0x0) { // 000x
                return decode_1001_000(op);
            } else if ((op & (b11 | b10 | b9)) == b9) { // 001x
                return decode_1001_001(op);
            } else if ((op & (b11 | b10 | b9)) == b10) { // 010x
                return decode_1001_010(op);
            } else if ((op & (b11 | b10 | b9)) == (b10 | b9)) { // 011x
                if ((op & b8) == 0) { //0110
                    return new AVRInstruction(AVRInstruction.Mnemonic.ADIW, pc);
                } else { //0111
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBIW, pc);
                }
            } else if ((op & (b11 | b10 | b9)) == b11) { // 100x
                if ((op & b8) == 0) { // 1000
                    return new AVRInstruction(AVRInstruction.Mnemonic.CBI, pc);
                } else { // 1001
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBCI, pc);
                }
            } else if ((op & (b11 | b10 | b9)) == (b11 | b9)) { // 101x
                if ((op & b8) == 0) { // 1010
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBI, pc);
                } else { // 1011
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBIS, pc);
                }
            } else if ((op & (b11 | b10)) == (b11 | b10)) { // 11xx
                return new AVRInstruction(AVRInstruction.Mnemonic.MUL, pc);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_000(ushort op) {
            ushort low = (ushort)(op & (b3 | b2 | b1 | b0)); // lowest order nibble of opcode

            if (low == 0x0) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDS, pc); //XXX: 32 bit
            } else if (low == 0x1) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDZ2, pc);
            } else if (low == 0x2) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDZ3, pc);
            } else if (low == 0x4) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LPM2, pc);
            } else if (low == 0x5) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LPM3, pc);
            } else if (low == 0x6) {
                return new AVRInstruction(AVRInstruction.Mnemonic.ELPM2, pc);
            } else if (low == 0x7) {
                return new AVRInstruction(AVRInstruction.Mnemonic.ELPM3, pc);
            } else if (low == 0x9) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDY2, pc);
            } else if (low == 0xA) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDY3, pc);
            } else if (low == 0xC) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDX1, pc);
            } else if (low == 0xD) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDX2, pc);
            } else if (low == 0xE) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LDX3, pc);
            } else if (low == 0xF) {
                return new AVRInstruction(AVRInstruction.Mnemonic.POP, pc);
            }
            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_001(ushort op) {
            // LAC, LAS, LAT, STS, XCH
            ushort low = (ushort)(op & (b3 | b2 | b1 | b0)); // lowest order nibble of opcode

            if (low == 0x0) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STS, pc); // 32 bit
            } else if (low == 0x1) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STZ2, pc);
            } else if (low == 0x2) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STZ3, pc);
            } else if (low == 0x4) {
                return new AVRInstruction(AVRInstruction.Mnemonic.XCH, pc);
            } else if (low == 0x5) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LAS, pc);
            } else if (low == 0x6) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LAC, pc);
            } else if (low == 0x7) {
                return new AVRInstruction(AVRInstruction.Mnemonic.LAT, pc);
            } else if (low == 0x9) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STY2, pc);
            } else if (low == 0xA) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STY3, pc);
            } else if (low == 0xC) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STX1, pc);
            } else if (low == 0xD) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STX2, pc);
            } else if (low == 0xE) {
                return new AVRInstruction(AVRInstruction.Mnemonic.STX3, pc);
            } else if (low == 0xF) {
                return new AVRInstruction(AVRInstruction.Mnemonic.PUSH, pc);
            }
             
            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_010(ushort op) {
            ushort low = (ushort) (op & (b3 | b2 | b1 | b0)); // lowest order nibble of pccode
            if (low == 0x0) { // 0000
                return new AVRInstruction(AVRInstruction.Mnemonic.COM, pc);
            } else if (low == b0) { // 0001
                return new AVRInstruction(AVRInstruction.Mnemonic.NEG, pc);
            } else if (low == b1) { // 0010
                return new AVRInstruction(AVRInstruction.Mnemonic.SWAP, pc);
            } else if (low == (b1 | b0)) { // 0011
                return new AVRInstruction(AVRInstruction.Mnemonic.INC, pc);
            } else if (low == (b2 | b0)) { // 0101
                return new AVRInstruction(AVRInstruction.Mnemonic.ASR, pc);
            } else if (low == (b2 | b1 | b0)) { // 0111
                return new AVRInstruction(AVRInstruction.Mnemonic.ROR, pc);
            } else if (low == (b3)) { // 1000
                return decode_1001_010x_xxxx_1000(op);
            } else if (low == (b3 | b0)) { // 1001
                if (isBitSet(op, b8)) { // *ICALL: 1001 0101 xxxx 1001
                    if (isBitSet(op, b4)) { // 1001 0101 xxx1 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.EICALL, pc);
                    } else { // 1001 0101 xxx0 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.ICALL, pc);
                    }
                } else { // *IJMP: 1001 0100 xxxx 1001
                    if (isBitSet(op, b4)) { // 1001 0100 xxx1 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.EIJMP, pc);
                    } else { // 1001 0100 xxx0 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.IJMP, pc);
                    }
                }
            } else if (low == (b3 | b1)) { // 1010
                return new AVRInstruction(AVRInstruction.Mnemonic.DEC, pc);
            } else if (low == (b3 | b1 | b0)) { // 1011
                return new AVRInstruction(AVRInstruction.Mnemonic.DES, pc);
            } else if (low == (b3 | b2) || low == (b3 | b2 | b0)) { // 110x
                return new AVRInstruction(AVRInstruction.Mnemonic.JMP, pc);
            } else if (low == (b3 | b2 | b1) || low == (b3 | b2 | b1 | b0)) { // 111x
                return new AVRInstruction(AVRInstruction.Mnemonic.CALL, pc);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_010x_xxxx_1000(ushort op) {
            if (!isBitSet(op, b8) && !isBitSet(op, b7)) {
                return new AVRInstruction(AVRInstruction.Mnemonic.BSET, pc);
            } else if (!isBitSet(op, b8) && isBitSet(op, b7)) {
                return new AVRInstruction(AVRInstruction.Mnemonic.BCLR, pc);
            } else if (isBitSet(op, b8) && !isBitSet(op, b7)) {
                if (isBitSet(op, b4)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.RETI, pc);
                } else {
                    return new AVRInstruction(AVRInstruction.Mnemonic.RET, pc);
                }
            } else if (isBitSet(op, b8) && isBitSet(op, b7)) { // 1001 0101 1xxx 1000
                ushort mask = (ushort) (op & 0x0070);
                if (mask == 0x0) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.SLEEP, pc);
                } else if (mask == 0x1) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.BREAK, pc);
                } else if (mask == 0x4) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.LPM1, pc);
                } else if (mask == 0x5) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.ELPM1, pc);
                } else if (mask == 0x6 || mask == 0x7) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.SPM, pc);
                }
            }
            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1011(ushort op) {
            if ((op & b11) == 0) { // select top bit of 3rd byte; input on 0, output on 1
                //IN
                return new AVRInstruction(AVRInstruction.Mnemonic.IN, pc);
            } else {
                //OUT
                return new AVRInstruction(AVRInstruction.Mnemonic.OUT, pc);
            }
        }

        AVRInstruction decode_1111(ushort op) {
            if ((op & (b11 | b10)) == 0x0) { // 1111 00xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BRBS, pc);
            } else if ((op & (b11 | b10)) == b10) { // 1111 01xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BRBC, pc);
            } else if ((op & (b11 | b10)) == b11) { // 1111 10xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BLD, pc);
            } else if ((op & (b11 | b10)) == (b11 | b10)) { // 1111 11xx
                if (isBitSet(op, b9)) { // 1111 111x
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBRS, pc);
                } else { // 1111 110x
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBRC, pc);
                }
            }

            throw new InvalidInstructionException("Unreachable");
        }

        public AVRInstruction decode() {
            ushort op = pc.readWord();
            ushort op12to16 = (ushort)((op & 0xf000)); // first 4 bits

            if (op12to16 == 0x0000) {
                return decode_0000(op);
            } else if (op12to16 == 0x1000) {
                return decode_0001(op);
            } else if (op12to16 == 0x2000) {
                return decode_0010(op);
            } else if (op12to16 == 0x3000) {
                //CPI
                return new AVRInstruction(AVRInstruction.Mnemonic.CPI, pc);
            } else if (op12to16 == 0x4000) {
                //SBCI
                return new AVRInstruction(AVRInstruction.Mnemonic.SBCI, pc);
            } else if (op12to16 == 0x5000) {
                //SUBI
                return new AVRInstruction(AVRInstruction.Mnemonic.SUBI, pc);
            } else if (op12to16 == 0x6000) {
                //ORI
                return new AVRInstruction(AVRInstruction.Mnemonic.ORI, pc);
            } else if (op12to16 == 0x7000) {
                //ANDI
                return new AVRInstruction(AVRInstruction.Mnemonic.ANDI, pc);
            } else if (op12to16 == 0x8000 || op12to16 == 0xA000) {
                return decode_10x0(op);
            } else if (op12to16 == 0x9000) {
                return decode_1001(op);
            } else if (op12to16 == 0xB000) {
                return decode_1011(op);
            } else if (op12to16 == 0xC000) {
                //RJMP
                return new AVRInstruction(AVRInstruction.Mnemonic.RJMP, pc);
            } else if (op12to16 == 0xD000) {
                //RCALL
                return new AVRInstruction(AVRInstruction.Mnemonic.RCALL, pc);
            } else if (op12to16 == 0xE000) {
                //LDI
                return new AVRInstruction(AVRInstruction.Mnemonic.LDI, pc);
            } else if (op12to16 == 0xF000) {
                //!!!!
                return decode_1111(op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        public void execute(AVRInstruction inst) {
            inst.execute(this);
        }
    }
}