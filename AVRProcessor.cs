using System;
using System.Collections;

namespace Arduino
{
    public class AVRProcessor : Processor
    {
        Memory memory;
        AVRStatusRegister status;
        byte[] dataReg;

        byte[] stackPtr; // 1 = high, 0 = low

        ushort pc; // 12 bits

        public AVRProcessor(Memory mem) {
            memory = mem;
            dataReg = new byte[32];
            stackPtr = new byte[2];
        }

        public byte getRegister(int i) {
            return dataReg[i];
        }

        public void setRegister(int i, byte val) {
            dataReg[i] = val;
        }

        public short getXRegister(int i) {
            return (short)(dataReg[26 + i * 2] + dataReg[27 + i * 2] << 8);
        }

        public void setXRegister(int i, short val) {
            dataReg[26 + i * 2] = (byte)(val & 0xff);
            dataReg[27 + i * 2] = (byte)(val >> 8);
        }

        ushort readOp() {
            return memory.readWord(pc);
        }

        public void incPC() {
            pc+=2; // jump forward 2 bytes
            pc &= 0x0fff; // pc is 12 bits
        }

        byte extractOp1RegId(short op) {
            return (byte)(op & 0x01f0);
        }

        byte extractOp2RegId(short op) {
            return (byte)(op & 0x020f);
        }

        // always between 16-31
        byte extract4BitRegId(short op) {
            return (byte)(16 + (op & 0x00f0));
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

        static bool isBitSet(ushort op, ushort bit) {
            return (op & bit) != 0;
        }

        AVRInstruction decode_0000(ushort op) {
            if (!isBitSet(op, b11) && !isBitSet(op, b10)) {
                if (isBitSet(op,b9)) {
                    return decode_0000_001(op); // *MUL* type instructions
                } else {
                    return new AVRInstruction(AVRInstruction.Mnemonic.MOVW, op);
                }
            } else if (!isBitSet(op, b11) && isBitSet(op, b10)) {
                //CPC
                return new AVRInstruction(AVRInstruction.Mnemonic.CPC, op);
            } else if (isBitSet(op, b11) && !isBitSet(op, b10)) {
                //SBC
                return new AVRInstruction(AVRInstruction.Mnemonic.SBC, op);
            } else if (isBitSet(op, b11) && isBitSet(op, b10)) {
                //ADD
                return new AVRInstruction(AVRInstruction.Mnemonic.ADD, op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        // *MUL
        AVRInstruction decode_0000_001(ushort op) {
            if (isBitSet(op, b8)) {
                return new AVRInstruction(AVRInstruction.Mnemonic.MULS, op);
            } else {
                if (!isBitSet(op, b7) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.MULSU, op);
                } else if (!isBitSet(op, b7) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMUL, op);
                } else if (isBitSet(op, b7) && !isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMULS, op);
                } else if (isBitSet(op, b7) && isBitSet(op, b3)) {
                    return new AVRInstruction(AVRInstruction.Mnemonic.FMULSU, op);
                }
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_0001(ushort op) {
            ushort op10to12 = (ushort)((op & (b10 | b11)));
            if (op10to12 == 0x0000) {
                //CPSE
                return new AVRInstruction(AVRInstruction.Mnemonic.CPSE, op);
            } else if (op10to12 == 0x0400) {
                //CP
                return new AVRInstruction(AVRInstruction.Mnemonic.CP, op);
            } else if (op10to12 == 0x0800) {
                //SUB
                return new AVRInstruction(AVRInstruction.Mnemonic.SUB, op);
            } else if (op10to12 == 0x0c00) {
                //ADC
                return new AVRInstruction(AVRInstruction.Mnemonic.ADC, op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_0010(ushort op) {
            ushort op10to12 = (ushort)((op & (b10 | b11)));
            if (op10to12 == 0x0000) {
                //AND
                return new AVRInstruction(AVRInstruction.Mnemonic.AND, op);
            } else if (op10to12 == 0x0400) {
                //EOR
                return new AVRInstruction(AVRInstruction.Mnemonic.EOR, op);
            } else if (op10to12 == 0x0800) {
                //MOV
                return new AVRInstruction(AVRInstruction.Mnemonic.MOV, op);
            } else if (op10to12 == 0x0c00) {
                //OR
                return new AVRInstruction(AVRInstruction.Mnemonic.OR, op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001(ushort op) {
            if ((op & (b11 | b10 | b9)) == 0x0) { // 000x
                // TODO:
                // LPM, POP, ELPM
            } else if ((op & (b11 | b10 | b9)) == b9) { // 001x
                // LAC, LAS, LAT, STS, XCH
            } else if ((op & (b11 | b10 | b9)) == b10) { // 010x
                return decode_1001_010(op);
            } else if ((op & (b11 | b10 | b9)) == (b10 | b9)) { // 011x
                if ((op & b8) == 0) { //0110
                    return new AVRInstruction(AVRInstruction.Mnemonic.ADIW, op);
                } else { //0111
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBIW, op);
                }
            } else if ((op & (b11 | b10 | b9)) == b11) { // 100x
                if ((op & b8) == 0) { // 1000
                    return new AVRInstruction(AVRInstruction.Mnemonic.CBI, op);
                } else { // 1001
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBCI, op);
                }
            } else if ((op & (b11 | b10 | b9)) == (b11 | b9)) { // 101x
                if ((op & b8) == 0) { // 1010
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBI, op);
                } else { // 1011
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBIS, op);
                }
            } else if ((op & (b11 | b10)) == (b11 | b10)) { // 11xx
                return new AVRInstruction(AVRInstruction.Mnemonic.MUL, op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_010(ushort op) {
            ushort low = (ushort) (op & (b3 | b2 | b1 | b0)); // lowest order nibble of opcode
            if (low == 0x0) { // 0000
                return new AVRInstruction(AVRInstruction.Mnemonic.COM, op);
            } else if (low == b0) { // 0001
                return new AVRInstruction(AVRInstruction.Mnemonic.NEG, op);
            } else if (low == b1) { // 0010
                return new AVRInstruction(AVRInstruction.Mnemonic.SWAP, op);
            } else if (low == (b1 | b0)) { // 0011
                return new AVRInstruction(AVRInstruction.Mnemonic.INC, op);
            } else if (low == (b2 | b0)) { // 0101
                return new AVRInstruction(AVRInstruction.Mnemonic.ASR, op);
            } else if (low == (b2 | b1 | b0)) { // 0111
                return new AVRInstruction(AVRInstruction.Mnemonic.ROR, op);
            } else if (low == (b3)) { // 1000
                return decode_1001_010x_xxxx_1000(op);
            } else if (low == (b3 | b0)) { // 1001
                if (isBitSet(op, b8)) { // *ICALL: 1001 0101 xxxx 1001
                    if (isBitSet(op, b4)) { // 1001 0101 xxx1 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.EICALL, op);
                    } else { // 1001 0101 xxx0 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.ICALL, op);
                    }
                } else { // *IJMP: 1001 0100 xxxx 1001
                    if (isBitSet(op, b4)) { // 1001 0100 xxx1 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.EIJMP, op);
                    } else { // 1001 0100 xxx0 1001
                        return new AVRInstruction(AVRInstruction.Mnemonic.IJMP, op);
                    }
                }
            } else if (low == (b3 | b1)) { // 1010
                return new AVRInstruction(AVRInstruction.Mnemonic.DEC, op);
            } else if (low == (b3 | b1 | b0)) { // 1011
                return new AVRInstruction(AVRInstruction.Mnemonic.DES, op);
            } else if (low == (b3 | b2) || low == (b3 | b2 | b0)) { // 110x
                return new AVRInstruction(AVRInstruction.Mnemonic.JMP, op);
            } else if (low == (b3 | b2 | b1) || low == (b3 | b2 | b1 | b0)) { // 111x
                return new AVRInstruction(AVRInstruction.Mnemonic.CALL, op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1001_010x_xxxx_1000(ushort op) {
            //TODO:
            //BCLR, BREAK, BSET, ELPM, RET, RETI, SLEEP, SPM
            throw new InvalidInstructionException("Unreachable");
        }

        AVRInstruction decode_1011(ushort op) {
            if ((op & b11) == 0) { // select top bit of 3rd byte; input on 0, output on 1
                //IN
                return new AVRInstruction(AVRInstruction.Mnemonic.IN, op);
            } else {
                //OUT
                return new AVRInstruction(AVRInstruction.Mnemonic.OUT, op);
            }
        }

        AVRInstruction decode_1111(ushort op) {
            if ((op & (b11 | b10)) == 0x0) { // 1111 00xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BRBS, op);
            } else if ((op & (b11 | b10)) == b10) { // 1111 01xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BRBC, op);
            } else if ((op & (b11 | b10)) == b11) { // 1111 10xx
                return new AVRInstruction(AVRInstruction.Mnemonic.BLD, op);
            } else if ((op & (b11 | b10)) == (b11 | b10)) { // 1111 11xx
                if (isBitSet(op, b9)) { // 1111 111x
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBRS, op);
                } else { // 1111 110x
                    return new AVRInstruction(AVRInstruction.Mnemonic.SBRC, op);
                }
            }

            throw new InvalidInstructionException("Unreachable");
        }

        public AVRInstruction decode() {
            ushort op = readOp();
            ushort op12to16 = (ushort)((op & 0xf000)); // first 4 bits

            if (op12to16 == 0x0000) {
                return decode_0000(op);
            } else if (op12to16 == 0x1000) {
                return decode_0001(op);
            } else if (op12to16 == 0x2000) {
                return decode_0010(op);
            } else if (op12to16 == 0x3000) {
                //CPI
                new AVRInstruction(AVRInstruction.Mnemonic.CPI, op);
            } else if (op12to16 == 0x4000) {
                //SBCI
                new AVRInstruction(AVRInstruction.Mnemonic.SBCI, op);
            } else if (op12to16 == 0x5000) {
                //SUBI
                new AVRInstruction(AVRInstruction.Mnemonic.SUBI, op);
            } else if (op12to16 == 0x6000) {
                //ORI
                new AVRInstruction(AVRInstruction.Mnemonic.ORI, op);
            } else if (op12to16 == 0x7000) {
                //ANDI
                new AVRInstruction(AVRInstruction.Mnemonic.ANDI, op);
            } else if (op12to16 == 0x8000) {
                //LDD/STD
                //TODO
            } else if (op12to16 == 0x9000) {
                return decode_1001(op);
            } else if (op12to16 == 0xA000) {
                //LD16/ST16
                //TODO
            } else if (op12to16 == 0xB000) {
                return decode_1011(op);
            } else if (op12to16 == 0xC000) {
                //RJMP
                return new AVRInstruction(AVRInstruction.Mnemonic.RJMP, op);
            } else if (op12to16 == 0xD000) {
                //RCALL
                return new AVRInstruction(AVRInstruction.Mnemonic.RCALL, op);
            } else if (op12to16 == 0xE000) {
                //LDI
                return new AVRInstruction(AVRInstruction.Mnemonic.LDI, op);
            } else if (op12to16 == 0xF000) {
                //!!!!
                return decode_1111(op);
            }

            throw new InvalidInstructionException("Unreachable");
        }

        public void execute(AVRInstruction inst) {
            inst.execute(this);
            if (inst.getMnemonic() == AVRInstruction.Mnemonic.JMP  ||
                inst.getMnemonic() == AVRInstruction.Mnemonic.CALL ||
                inst.getMnemonic() == AVRInstruction.Mnemonic.LDS  ||
                inst.getMnemonic() == AVRInstruction.Mnemonic.STS) {
                incPC();
            }
            incPC();
        }
    }
}