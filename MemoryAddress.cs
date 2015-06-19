using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arduino
{
    public class MemoryAddress
    {
        Memory memory;
        uint address;

        public MemoryAddress(Memory mem, uint addr) {
            memory = mem;
            address = addr;
        }

        public MemoryAddress(MemoryAddress o, int off = 0) {
            memory = o.memory;
            address = (uint) (o.address + off);
        }

        public uint value() {
            return address;
        }

        public byte readByte() {
            return (byte) (memory.readWord(address) & 0x000f);
        }

        public ushort readWord() {
            return memory.readWord(address);
        }

        public uint readLong() {
            return (uint) ((memory.readWord(address) << 16) | memory.readWord(address+2));
        }

        public void increment() {
            address += 2;
            address &= 0x0fff; // 12 bit address
        }

        public void decrement() {
            address -= 2;
        }

        public void jump(uint addr) {
            address = addr;
        }
    }
}
