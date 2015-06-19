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

        protected enum OpClass
        {
            Arithmetic,
            Bit,
            Transfer,
            Jump,
            Branch,
            Call,
            Other,
        }

        protected Mnemonic mnemonic;
        protected uint opcode;
        protected int op1;
        protected int op2;
        
        protected AVRInstruction() {
        }

        public AVRInstruction(Mnemonic mn) {
            mnemonic = mn;
            opcode = 0;
        }

        public AVRInstruction(Mnemonic mn, MemoryAddress addr) {
            mnemonic = mn;
            opcode = addr.readWord();

            switch (getOperandFormat()) {
                case OpFormat.K:
                    decode_KOperands(addr);
                    break;
                case OpFormat.KK:
                    decode_KKOperands(addr);
                    break;
                case OpFormat.R:
                    decode_ROperands(addr);
                    break;
                case OpFormat.RR:
                    decode_RROperands(addr);
                    break;
                case OpFormat.RK:
                    decode_RKOperands(addr);
                    break;
            }
        }

        public Mnemonic getMnemonic() {
            return mnemonic;
        }

        virtual public bool is32Bit() {
            return mnemonic == Mnemonic.CALL ||
                    mnemonic == Mnemonic.JMP ||
                    mnemonic == Mnemonic.LDS ||
                    mnemonic == Mnemonic.STS;
        }

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
                //case Mnemonic.STY1:
                case Mnemonic.STY2:
                case Mnemonic.STY3:
                //case Mnemonic.STZ1:
                case Mnemonic.STZ2:
                case Mnemonic.STZ3:
                case Mnemonic.LDX1:
                case Mnemonic.LDX2:
                case Mnemonic.LDX3:
                //case Mnemonic.LDY1:
                case Mnemonic.LDY2:
                case Mnemonic.LDY3:
                //case Mnemonic.LDZ1:
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

        protected OpClass getOperandClass() {
            switch (mnemonic) {
                case Mnemonic.ADD:
                case Mnemonic.ADC:
                case Mnemonic.ADIW:
                case Mnemonic.SUB:
                case Mnemonic.SUBI:
                case Mnemonic.SBC:
                case Mnemonic.SBCI:
                case Mnemonic.SBIW:
                case Mnemonic.AND:
                case Mnemonic.ANDI:
                case Mnemonic.OR:
                case Mnemonic.ORI:
                case Mnemonic.EOR:
                case Mnemonic.COM:
                case Mnemonic.NEG:
                case Mnemonic.INC:
                case Mnemonic.DEC:
                case Mnemonic.MUL:
                case Mnemonic.MULS:
                case Mnemonic.MULSU:
                case Mnemonic.FMUL:
                case Mnemonic.FMULS:
                case Mnemonic.FMULSU:
                case Mnemonic.DES:
                case Mnemonic.CP:
                case Mnemonic.CPC:
                case Mnemonic.CPI:
                case Mnemonic.LAS:
                case Mnemonic.LAC:
                case Mnemonic.LAT:
                case Mnemonic.LSR:
                case Mnemonic.ROR:
                case Mnemonic.ASR:
                case Mnemonic.SWAP:
                    return OpClass.Arithmetic;

                case Mnemonic.MOV:
                case Mnemonic.MOVW:
                case Mnemonic.LDI:
                case Mnemonic.LDX1:
                case Mnemonic.LDX2:
                case Mnemonic.LDX3:
                case Mnemonic.LDY1:
                case Mnemonic.LDY2:
                case Mnemonic.LDY3:
                case Mnemonic.LDDY:
                case Mnemonic.LDZ1:
                case Mnemonic.LDZ2:
                case Mnemonic.LDZ3:
                case Mnemonic.LDDZ:
                case Mnemonic.LDS:
                case Mnemonic.LDS16:
                case Mnemonic.STX1:
                case Mnemonic.STX2:
                case Mnemonic.STX3:
                case Mnemonic.STY1:
                case Mnemonic.STY2:
                case Mnemonic.STY3:
                case Mnemonic.STDY:
                case Mnemonic.STZ1:
                case Mnemonic.STZ2:
                case Mnemonic.STZ3:
                case Mnemonic.STDZ:
                case Mnemonic.STS:
                case Mnemonic.STS16:
                case Mnemonic.LPM1:
                case Mnemonic.LPM2:
                case Mnemonic.LPM3:
                case Mnemonic.ELPM1:
                case Mnemonic.ELPM2:
                case Mnemonic.ELPM3:
                case Mnemonic.SPM:
                case Mnemonic.IN:
                case Mnemonic.OUT:
                case Mnemonic.PUSH:
                case Mnemonic.POP:
                case Mnemonic.XCH:
                    return OpClass.Transfer;


                case Mnemonic.SBRC:
                case Mnemonic.SBRS:
                case Mnemonic.SBIC:
                case Mnemonic.SBIS:
                case Mnemonic.BRBS:
                case Mnemonic.BRBC:
                case Mnemonic.CPSE:
                    return OpClass.Branch;

                case Mnemonic.SBR:
                case Mnemonic.CBR:
                case Mnemonic.BST:
                case Mnemonic.BLD:
                case Mnemonic.BSET:
                case Mnemonic.BCLR:
                case Mnemonic.SBI:
                case Mnemonic.CBI:
                    return OpClass.Bit;


                case Mnemonic.RCALL:
                case Mnemonic.ICALL:
                case Mnemonic.EICALL:
                case Mnemonic.CALL:
                case Mnemonic.RET:
                case Mnemonic.RETI:
                    return OpClass.Call;


                case Mnemonic.RJMP:
                case Mnemonic.IJMP:
                case Mnemonic.EIJMP:
                case Mnemonic.JMP:
                    return OpClass.Jump;

                case Mnemonic.BREAK:
                case Mnemonic.NOP:
                case Mnemonic.SLEEP:
                case Mnemonic.WDR:
                    return OpClass.Other;
            }

            throw new Exception("Could not identify opcode class");
        }

        protected void decode_ROperands(MemoryAddress addr) {
            ushort op = addr.readWord();
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
                    op1 = (byte)((op & 0x01F0) >> 4);
                    break;
                default:
                    throw new InvalidInstructionException("provided mnemonic is not R format");
            }
        }

        protected void decode_KOperands(MemoryAddress addr) {
            ushort op = addr.readWord();
            int iop = (int) addr.readLong();
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
                    // xxxx xxxk kkkk xxxk
                    // kkkk kkkk kkkk kkkk
                    // value is shifted left, because K is implicitly a multiple of 2
                    // (address is word oriented)
                    op1 = ((iop & 0x01F00000) >> 2) | ((iop & 0x0001FFFF) << 1);
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
                    op1 <<= 1; // shift 1 bit to the right, low bit of address is implicitly zero
                    break;
                default: 
                    throw new InvalidInstructionException("Provided Instruction does not conform to the K template: " + mnemonic);
            }
        }

        protected void decode_RROperands(MemoryAddress addr) {
            ushort op = addr.readWord();
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

        protected void decode_KKOperands(MemoryAddress addr) {
            throw new NotImplementedException("Decode KK ops not implemented: " + mnemonic);
        }

        protected void decode_RKOperands(MemoryAddress addr) {
            ushort op = addr.readWord();
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

                case Mnemonic.LDDY:
                case Mnemonic.LDDZ:
                case Mnemonic.STDY:
                case Mnemonic.STDZ:
                    op1 = (byte)((op & 0x01F0) >> 4);
                    op2 = (ushort)(((op & 0x2000) >> 8) | ((op & 0x0C00) >> 7) | (op & 0x0007));
                    break;
                //case Mnemonic.LDS:
                //case Mnemonic.STS:   
                //break;
                default:
                    throw new InvalidInstructionException("Provided Instruction does not conform to the RK template: " + mnemonic);
            }
        }

        bool hasCarry(byte b1, byte b2, byte bit) {
            byte bitmask = (byte) (0x01 << bit);
            byte sum = (byte) (b1 + b2);
            return (((b1 & b2) | (b1 & ~sum) | (~sum & b2)) & bitmask) != 0;
        }

        bool hasNCarry(byte b1, byte b2, byte bit) {
            byte bitmask = (byte)(0x01 << bit);
            byte sub = (byte)(b1 - b2);
            return (((b1 & b2) | (b1 & ~sub) | (~sub & b2)) & bitmask) != 0;
        }

        void executeCall(AVRProcessor proc) {
            ushort addr;
            switch (mnemonic) {
                case Mnemonic.CALL:
                    proc.getPC().increment();
                    proc.getPC().increment(); // 32 bit instruction
                    proc.pushStackWord((ushort) proc.getPC().value());
                    proc.setPC(new MemoryAddress(proc.getProgramMemory(), (uint) op1)); 
                    break;

                case Mnemonic.RCALL:
                    proc.getPC().increment();
                    proc.pushStackWord((ushort) proc.getPC().value());
                    proc.setPC(new MemoryAddress(proc.getProgramMemory(), (uint) (proc.getPC().value() + op1)));
                    break;

                case Mnemonic.ICALL:
                    proc.getPC().increment();
                    proc.pushStackWord((ushort) proc.getPC().value());
                    proc.setPC(new MemoryAddress(proc.getProgramMemory(), proc.readRegisterZ()));
                    break;

                case Mnemonic.EICALL:
                    throw new NotImplementedException("EICALL not implemented");

                case Mnemonic.RET:
                    addr = proc.popStackWord();
                    proc.setPC(new MemoryAddress(proc.getProgramMemory(), addr));
                    break;

                case Mnemonic.RETI:
                    byte status = proc.readStatus();
                    proc.writeStatus((byte) (status | 0x80)); // set I flag
                    addr = proc.popStackWord();
                    proc.setPC(new MemoryAddress(proc.getProgramMemory(), addr));
                    break;
            }
        }

        void executeArithmetic(AVRProcessor proc) {
            //TODO: STATUS flags
            int result = 0;
            bool writeEnable = true;
            bool writePair = false;
            switch (mnemonic) {
                case Mnemonic.ADD:
                    result = proc.readRegister((uint) op1) + proc.readRegister((uint) op2);
                    break;

                case Mnemonic.ADC:
                    result = proc.readRegister((uint)op1) + 
                                                         proc.readRegister((uint)op2) + 
                                                         proc.readStatus() & 0x01;
                    break;

                case Mnemonic.ADIW:
                    result = proc.readRegisterPair((uint)op1) +
                                                         op2;
                    writePair = true;
                    break;

                case Mnemonic.SUB:
                    result = proc.readRegister((uint)op1) - proc.readRegister((uint)op2);
                    break;

                case Mnemonic.SUBI:
                    result = proc.readRegister((uint)op1) - op2;
                    break;

                case Mnemonic.SBC:
                    result = proc.readRegister((uint)op1) -
                             proc.readRegister((uint)op2) -
                             proc.readStatus() & 0x01;
                    break;

                case Mnemonic.SBCI:
                    result = proc.readRegister((uint)op1) -
                             op2 -
                             proc.readStatus() & 0x01;
                    break;

                case Mnemonic.SBIW:
                    result = proc.readRegisterPair((uint)op1) - op2;
                    writePair = true;
                    break;

                case Mnemonic.AND:
                    result = proc.readRegister((uint)op1) & proc.readRegister((uint)op2);
                    break;

                case Mnemonic.ANDI:
                    result = proc.readRegister((uint)op1) & op2;
                    break;

                case Mnemonic.OR:
                    result = proc.readRegister((uint)op1) | proc.readRegister((uint)op2);
                    break;

                case Mnemonic.ORI:
                    result = proc.readRegister((uint)op1) | op2;
                    break;

                case Mnemonic.EOR:
                    result = proc.readRegister((uint)op1) ^ proc.readRegister((uint)op2);
                    break;

                case Mnemonic.COM:
                    result = ~proc.readRegister((uint)op1);
                    break;

                case Mnemonic.NEG:
                    result = -proc.readRegister((uint)op1);
                    break;

                case Mnemonic.INC:
                    result = proc.readRegister((uint)op1) + 1;
                    break;

                case Mnemonic.DEC:
                    result = proc.readRegister((uint)op1) - 1;
                    break;

                case Mnemonic.MUL:
                case Mnemonic.MULS:
                case Mnemonic.MULSU:
                case Mnemonic.FMUL:
                case Mnemonic.FMULS:
                case Mnemonic.FMULSU:
                case Mnemonic.DES:
                    throw new NotImplementedException("Instruction not yet implemented: " + mnemonic);

                case Mnemonic.CP:
                case Mnemonic.CPC:
                case Mnemonic.CPI:
                case Mnemonic.LAS:
                case Mnemonic.LAC:
                case Mnemonic.LAT:
                    throw new NotImplementedException("Instruction not yet implemented: " + mnemonic);

                case Mnemonic.LSR:
                     result = proc.readRegister((uint)op1) >> 1;
                     break;

                case Mnemonic.ROR:
                    result = (proc.readRegister((uint)op1) >> 1) | ((proc.readStatus() & 0x01) << 7); // carry flag is rotated into bit 7
                    break;

                case Mnemonic.ASR:
                     result = (((sbyte) proc.readRegister((uint)op1)) >> 1);
                     break;

                case Mnemonic.SWAP:
                    byte val = proc.readRegister((uint)op1);
                    result = (val << 4) | (val >> 4);
                    break;
            }

            // write back results
            if (writeEnable) {
                if (writePair) {
                    proc.writeRegisterPair((uint)op1, (ushort)result);
                } else {
                    proc.writeRegister((uint)op1, (byte)result);
                }
            }

            proc.getPC().increment();
        }

        void executeJump(AVRProcessor proc) {
            MemoryAddress target = null;
            switch (mnemonic) {
                case Mnemonic.JMP:
                    proc.getPC().increment(); // 32 bit instruction
                    target = new MemoryAddress(proc.getProgramMemory(), (uint) op1);
                    break;
                case Mnemonic.IJMP:
                    target = new MemoryAddress(proc.getProgramMemory(), proc.readRegisterZ()); // address in Z register
                    break;
                
                case Mnemonic.RJMP:
                    target = new MemoryAddress(proc.getPC(), op1); // get new memory address relative to PC
                    target.increment();
                    break;

                case Mnemonic.EIJMP:
                default:
                    throw new Exception("Operation not implemented");
            }

            proc.setPC(target);
        }

        void executeTransfer(AVRProcessor proc) {
            ushort addr;
            switch (mnemonic) {
                case Mnemonic.MOV:
                    proc.writeRegister((uint)op1, proc.readRegister((uint)op2));
                    break;
                case Mnemonic.MOVW:
                    proc.writeRegisterPair((uint)op1, proc.readRegisterPair((uint)op2));
                    break;

                case Mnemonic.LDI:
                    proc.writeRegister((uint) op1, (byte) op2);
                    break;

                case Mnemonic.LDX1:
                    addr = proc.readRegisterX();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.LDX2:
                    addr = proc.readRegisterX();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterX((ushort) (addr + 1));
                    break;

                case Mnemonic.LDX3:
                    addr = (ushort) (proc.readRegisterX() - 1);
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterX((ushort) (addr));
                    break;

                case Mnemonic.LDY1:
                    addr = proc.readRegisterY();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.LDY2:
                    addr = proc.readRegisterY();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterY((ushort) (addr + 1));
                    break;

                case Mnemonic.LDY3:
                    addr = (ushort) (proc.readRegisterY() - 1);
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterY((ushort) (addr));
                    break;

                case Mnemonic.LDDY:
                    addr = (ushort) (proc.readRegisterY() + op2);
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.LDZ1:
                    addr = proc.readRegisterZ();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.LDZ2:
                    addr = proc.readRegisterZ();
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterZ((ushort)(addr + 1));
                    break;

                case Mnemonic.LDZ3:
                    addr = (ushort)(proc.readRegisterZ() - 1);
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    proc.writeRegisterZ((ushort)(addr));
                    break;

                case Mnemonic.LDDZ:
                    addr = (ushort) (proc.readRegisterZ() + op2);
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.LDS:
                    addr = (ushort) (op2); //TODO: RAMPD
                    proc.writeRegister((uint)op1, proc.getDataMemory().readByte(addr)); 
                    proc.getPC().increment(); // 32 bit instruction
                    break;

                case Mnemonic.LDS16:
                    throw new NotImplementedException("LDS 16-bit not implemented");

                case Mnemonic.STX1:
                    addr = proc.readRegisterX();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint) op1));
                    break;

                case Mnemonic.STX2:
                    addr = proc.readRegisterX();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint) op1));
                    proc.writeRegisterX((ushort) (addr + 1));
                    break;

                case Mnemonic.STX3:
                    addr = (ushort) (proc.readRegisterX() - 1);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint) op1));
                    proc.writeRegisterX(addr);
                    break;

                case Mnemonic.STY1:
                    addr = proc.readRegisterY();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    break;

                case Mnemonic.STY2:
                    addr = proc.readRegisterY();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    proc.writeRegisterY((ushort)(addr + 1));
                    break;

                case Mnemonic.STY3:
                    addr = (ushort)(proc.readRegisterY() - 1);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    proc.writeRegisterY(addr);
                    break;

                case Mnemonic.STDY:
                    addr = (ushort) (proc.readRegisterY() + op2);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    break;

                case Mnemonic.STZ1:
                    addr = proc.readRegisterZ();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    break;

                case Mnemonic.STZ2:
                    addr = proc.readRegisterZ();
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    proc.writeRegisterZ((ushort)(addr + 1));
                    break;

                case Mnemonic.STZ3:
                    addr = (ushort)(proc.readRegisterZ() - 1);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    proc.writeRegisterZ(addr);
                    break;

                case Mnemonic.STDZ:
                    addr = (ushort)(proc.readRegisterZ() + op2);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    break;

                case Mnemonic.STS:
                    proc.getPC().increment(); // 32 bit instruction
                    break;

                case Mnemonic.STS16:
                    throw new NotImplementedException("LDS 16-bit not implemented");

                case Mnemonic.LPM1:
                    addr = (ushort)proc.readRegisterZ();
                    proc.writeRegister(0, proc.getProgramMemory().readByte(addr));
                    break;

                case Mnemonic.LPM2:
                    addr = (ushort)proc.readRegisterZ();
                    proc.writeRegister((uint) op1, proc.getProgramMemory().readByte(addr));
                    break;

                case Mnemonic.LPM3:
                    addr = (ushort)proc.readRegisterZ();
                    proc.writeRegister((uint) op1, proc.getProgramMemory().readByte(addr));
                    proc.writeRegisterZ((ushort) (addr + 1));
                    break;

                case Mnemonic.ELPM1:
                case Mnemonic.ELPM2:
                case Mnemonic.ELPM3:
                    throw new NotImplementedException("ELPM not implemented");

                case Mnemonic.SPM:
                    throw new NotImplementedException("SPM not implemented");

                case Mnemonic.IN:
                    addr = (ushort)(op2 + 0x20);
                    proc.writeRegister((uint) op1, proc.getDataMemory().readByte(addr));
                    break;

                case Mnemonic.OUT:
                    addr = (ushort)(op2 + 0x20);
                    proc.getDataMemory().writeByte(addr, proc.readRegister((uint)op1));
                    break;

                case Mnemonic.PUSH:
                case Mnemonic.POP:
                case Mnemonic.XCH:
                    break;
            }

            proc.getPC().increment();
        }

        void executeOther(AVRProcessor proc) {
            switch (mnemonic) {
                case Mnemonic.BREAK: // TODO: JTAG debugging?
                case Mnemonic.NOP:
                case Mnemonic.SLEEP: // TODO: handle MCU sleep
                case Mnemonic.WDR: // TODO: reset watchdog timer
                    proc.getPC().increment();
                    break;
            }
        }

        virtual public void execute(AVRProcessor proc) {
            switch (getOperandClass()) {
                case OpClass.Arithmetic:
                    executeArithmetic(proc);
                    break;

                case OpClass.Bit:
                case OpClass.Branch:
                    proc.getPC().increment();
                    break;

                case OpClass.Call:
                    executeCall(proc);
                    break;

                case OpClass.Jump:
                    executeJump(proc);
                    break;

                case OpClass.Transfer:
                    executeTransfer(proc);
                    break;

                case OpClass.Other:
                    executeOther(proc);
                    break;

                default:
                    proc.getPC().increment();
                    break;
            }
        }

        public override string ToString() {
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