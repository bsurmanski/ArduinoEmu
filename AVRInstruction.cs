using System;
using System.Collections;

namespace Arduino
{
    public class AVRInstruction
    {
        public enum Mnemonic
        {
            ADD,
            ADC,
            ADIW,
            SUB,
            SUBI,
            SBC,
            SBCI,
            SBIW,
            AND,
            ANDI,
            OR,
            ORI,
            EOR,
            COM,
            NEG,
            SBR,
            CBR,
            INC,
            DEC,
            TST,
            CLR,
            SER,
            MUL,
            MULS,
            MULSU,
            FMUL,
            FMULS,
            FMULSU,
            DES,
            RJMP,
            IJMP,
            EIJMP,
            JMP,
            RCALL,
            ICALL,
            EICALL,
            CALL,
            RET,
            RETI,
            CPSE,
            CP,
            CPC,
            CPI,
            SBRC,
            SBRS,
            SBIC,
            SBIS,
            BRBS,
            BRBC,
            BREQ,
            BRNE,
            BRCS,
            BRCC,
            BRSH,
            BRLO,
            BRMI,
            BRPL,
            BRGE,
            BRLT,
            BRHS,
            BRHC,
            BRTS,
            BRTC,
            BRVS,
            BRVC,
            BRIE,
            BRID,
            MOV,
            MOVW,
            LDI,

            LDX1,
            LDX2,
            LDX3,
            LDY1,
            LDY2,
            LDY3,
            LDDY,
            LDZ1,
            LDZ2,
            LDZ3,
            LDDZ,

            LDS,
            LDS16,

            STX1,
            STX2,
            STX3,
            STY1,
            STY2,
            STY3,
            STDY,
            STZ1,
            STZ2,
            STZ3,
            STDZ,

            STS,
            STS16,

            LPM1,
            LPM2,
            LPM3,
            ELPM1,
            ELPM2,
            ELPM3,

            SPM,

            IN,
            OUT,
            PUSH,
            POP,
            XCH,
            LAS,
            LAC,
            LAT,
            LSL,
            LSR,
            ROL,
            ROR,
            ASR,
            SWAP,
            BSET,
            BCLR,
            SBI,
            CBI,
            BST,
            BLD,
            BREAK,
            NOP,
            SLEEP,
            WDR,
        }

        protected enum OpFormat
        {
            R, // takes a single register
            K, // takes a constant
            RR, // takes 2 registers
            KK, // takes 2 constants
            RK, // takes a register and constant
            NUL // takes no ops (nullary)
        }

        protected Mnemonic mnemonic;
        protected uint opcode;
        protected int op1;
        protected int op2;

        protected OpFormat getOperandFormat() {
            switch (mnemonic) {
                case Mnemonic.COM:
                case Mnemonic.NEG:
                case Mnemonic.INC:
                case Mnemonic.DEC:
                case Mnemonic.PUSH:
                case Mnemonic.POP:
                case Mnemonic.LSR:
                case Mnemonic.ROR:
                case Mnemonic.ASR:
                case Mnemonic.SWAP:
                case Mnemonic.STX1:
                case Mnemonic.STX2:
                case Mnemonic.STX3:
                case Mnemonic.STY1:
                case Mnemonic.STY2:
                case Mnemonic.STY3:
                case Mnemonic.STZ1:
                case Mnemonic.STZ2:
                case Mnemonic.STZ3:
                case Mnemonic.LDX1:
                case Mnemonic.LDX2:
                case Mnemonic.LDX3:
                case Mnemonic.LDY1:
                case Mnemonic.LDY2:
                case Mnemonic.LDY3:
                case Mnemonic.LDZ1:
                case Mnemonic.LDZ2:
                case Mnemonic.LDZ3:
                case Mnemonic.LAC:
                case Mnemonic.LAS:
                case Mnemonic.LAT:
                case Mnemonic.LPM2:
                case Mnemonic.LPM3:
                case Mnemonic.ELPM2:
                case Mnemonic.ELPM3:
                    return OpFormat.R;

                case Mnemonic.BSET:
                case Mnemonic.BCLR:
                case Mnemonic.DES:
                case Mnemonic.CALL:
                case Mnemonic.JMP:
                case Mnemonic.RCALL:
                case Mnemonic.RJMP:
                    return OpFormat.K;

                case Mnemonic.ADC:
                case Mnemonic.ADD:
                case Mnemonic.AND:
                case Mnemonic.CP:
                case Mnemonic.CPC:
                case Mnemonic.CPSE:
                case Mnemonic.EOR:
                case Mnemonic.MOV:
                case Mnemonic.MUL:
                case Mnemonic.OR:
                case Mnemonic.SBC:
                case Mnemonic.SUB:
                case Mnemonic.FMUL:
                case Mnemonic.FMULS:
                case Mnemonic.FMULSU:
                case Mnemonic.MULSU:
                case Mnemonic.MOVW:
                case Mnemonic.MULS:
                    return OpFormat.RR;

                case Mnemonic.BRBC:
                case Mnemonic.BRBS:
                case Mnemonic.CBI:
                case Mnemonic.SBI:
                case Mnemonic.SBIC:
                case Mnemonic.SBIS:
                    return OpFormat.KK;

                case Mnemonic.ADIW:
                case Mnemonic.SBIW:
                case Mnemonic.ANDI:
                case Mnemonic.CPI:
                case Mnemonic.SUBI:
                case Mnemonic.LDI:
                case Mnemonic.ORI:
                case Mnemonic.SBCI:
                case Mnemonic.SBR:
                case Mnemonic.BLD:
                case Mnemonic.BST:
                case Mnemonic.SBRC:
                case Mnemonic.SBRS:
                case Mnemonic.IN:
                case Mnemonic.OUT:
                case Mnemonic.LDDY:
                case Mnemonic.LDDZ:
                case Mnemonic.LDS:
                case Mnemonic.LDS16:
                case Mnemonic.STDY:
                case Mnemonic.STDZ:
                case Mnemonic.STS:
                case Mnemonic.STS16:
                    return OpFormat.RK;

                case Mnemonic.BREAK:
                case Mnemonic.EICALL:
                case Mnemonic.EIJMP:
                case Mnemonic.LPM1:
                case Mnemonic.ELPM1:
                case Mnemonic.NOP:
                case Mnemonic.RET:
                case Mnemonic.RETI:
                case Mnemonic.SLEEP:
                case Mnemonic.WDR:
                case Mnemonic.SPM:
                    return OpFormat.NUL;

                default:
                    throw new Exception("Unknown op mnemonic: " + mnemonic.ToString());
            }
        }

        protected AVRInstruction() {
        }

        public AVRInstruction(Mnemonic mn) {
            mnemonic = mn;
            opcode = 0;
        }

        protected void decode_ROperands(ushort op) {
            switch (mnemonic) {
                case Mnemonic.COM:
                case Mnemonic.NEG:
                case Mnemonic.INC:
                case Mnemonic.DEC:
                case Mnemonic.PUSH:
                case Mnemonic.POP:
                case Mnemonic.LSR:
                case Mnemonic.ROR:
                case Mnemonic.ASR:
                case Mnemonic.SWAP:
                    // xxxx xxxd dddd xxxx
                    op1 = (byte)(op & 0x01F0 >> 4);
                    break;
                default:
                    throw new InvalidInstructionException("provided mnemonic is not R format");
            }
        }

        protected void decode_KOperands(ushort op) {
            switch (mnemonic) {
                case Mnemonic.BSET:
                case Mnemonic.BCLR:
                    // xxxx xxxx xkkk xxxx
                    op1 = (op & 0x0070) >> 4;
                    break;
                case Mnemonic.DES:
                    // xxxx xxxx kkkk xxxx
                    op1 = (op & 0x00F0) >> 4;
                    break;
                case Mnemonic.CALL:
                case Mnemonic.JMP:
                    //TODO: 2 byte
                    break;
                case Mnemonic.RCALL:
                case Mnemonic.RJMP:
                    // xxxx kkkk kkkk kkkk
                    // -2K <= k <= 2k
                    op1 = (op & 0x0FFF);
                    if ((op & 0x8000) != 0) {
                        op1 <<= 20; // sign extend to 32 bits
                        op1 >>= 20; // this will put sign bit in top bit and ASR will keep sign for intermediate bits
                    }
                    break;
                default: 
                    throw new InvalidInstructionException("Provided Instruction does not conform to the K template: " + mnemonic);
            }
        }

        protected void decode_RROperands(ushort op) {
            switch (mnemonic) {
                case Mnemonic.ADC:
                case Mnemonic.ADD:
                case Mnemonic.AND:
                case Mnemonic.CP:
                case Mnemonic.CPC:
                case Mnemonic.CPSE:
                case Mnemonic.EOR:
                case Mnemonic.MOV:
                case Mnemonic.MUL:
                case Mnemonic.OR:
                case Mnemonic.SBC:
                case Mnemonic.SUB:
                    // xxxx xxrd dddd rrrr
                    op1 = (byte)((op & 0x01F0) >> 4);
                    op2 = (byte)((op & 0x000F) | ((op & 0x0200) >> 5));
                    break;

                case Mnemonic.FMUL:
                case Mnemonic.FMULS:
                case Mnemonic.FMULSU:
                case Mnemonic.MULSU:
                    // xxxx xxxx xddd xrrr
                    op1 = (byte)(16 + (op & 0x0070) >> 4);
                    op2 = (byte)(16 + (op & 0x0007));
                    break;
                case Mnemonic.MOVW:
                    // xxxx xxxx dddd rrrr
                    op1 = (byte)(2 * ((op & 0x00F0) >> 4));
                    op2 = (byte)(2 * (op & 0x000F));
                    break;

                case Mnemonic.MULS:
                    // xxxx xxxx dddd rrrr
                    // d + 16, r + 16
                    op1 = (byte)(16 + (op & 0x00F0) >> 4);
                    op2 = (byte)(16 + (op & 0x000F));
                    break;
                default:
                    throw new InvalidInstructionException("Provided Instruction does not conform to the 2R template: " + mnemonic);
            }
        }

        protected void decode_KKOperands(ushort op) {

        }

        protected void decode_RKOperands(ushort op) {
            switch (mnemonic) {
                case Mnemonic.ADIW:
                case Mnemonic.SBIW:
                    // xxxx xxxx kkrr kkkk
                    // r = {24,26,28,30}; 0 <= k <= 63
                    op1 = (byte)(24 + 2 * ((op & 0x0030) >> 4));
                    op2 = (short)(((op & 0x00C0) >> 2) | (op & 0x000F));
                    break;
                case Mnemonic.ANDI:
                case Mnemonic.CPI:
                case Mnemonic.SUBI:
                case Mnemonic.LDI:
                case Mnemonic.ORI:
                case Mnemonic.SBCI:
                case Mnemonic.SBR:
                    // xxxx kkkk rrrr kkkk 
                    // 16 <= r <= 31; 0 <= k <= 255
                    op1 = (byte)(16 + ((op & 0x00F0) >> 4));
                    op2 = (short)(((op & 0x0F00) >> 4) | (op & 0x000F));
                    break;
                case Mnemonic.BLD:
                case Mnemonic.BST:
                case Mnemonic.SBRC:
                case Mnemonic.SBRS:
                    // xxxx xxxr rrrr xkkk
                    op1 = (byte)((op & 0x01F0) >> 4);
                    op2 = (short)(op & 0x0007);
                    break;
                case Mnemonic.IN:
                case Mnemonic.OUT:
                    // xxxx xkkr rrrr kkkk
                    op1 = (byte)((op & 0x01F0) >> 4);
                    op2 = (short)(((op & 0x0600) >> 5) | (op & 0x000F));
                    break;
                //case Mnemonic.LDD: (iv)
                //case Mnemonic.LDS:
                //case Mnemonic.STD: (iv)
                //case Mnemonic.STS:    
                //break;
                default:
                    throw new InvalidInstructionException("Provided Instruction does not conform to the RK template: " + mnemonic);
            }
        }

        public AVRInstruction(Mnemonic mn, uint op) {
            mnemonic = mn;
            opcode = op;
        }

        public Mnemonic getMnemonic() {
            return mnemonic;
        }

        virtual public bool is32Bit() {
            return  mnemonic == Mnemonic.CALL || 
                    mnemonic == Mnemonic.JMP || 
                    mnemonic == Mnemonic.LDS ||
                    mnemonic == Mnemonic.STS;
        }

        virtual public void execute(AVRProcessor processor) {

        }

        public string ToString() {
            switch (getOperandFormat()) {
                case OpFormat.R:
                    return mnemonic.ToString() + " R" + op1;
                case OpFormat.K:
                    return mnemonic.ToString() + " " + op1;
                case OpFormat.RR:
                    return mnemonic.ToString() + " R" + op1 + ",R" + op2;
                case OpFormat.KK:
                    return mnemonic.ToString() + " " + op1 + "," + op2;
                case OpFormat.RK:
                    return mnemonic.ToString() + " R" + op1 + "," + op2;
                case OpFormat.NUL:
                    return mnemonic.ToString();
                default:
                    throw new InvalidInstructionException("Provided Instruction has an invalid operand format: " + mnemonic);
            }
        }
    }
}