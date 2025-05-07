//以下是垃圾代码，闲人勿入
namespace EscudeTools
{
    public static class Define
    {
        public const byte INST_POP = 1;
        public const byte INST_POP_N = 2;
        public const byte INST_POP_RET = 3;
        public const byte INST_PUSH_INT = 4;
        public const byte INST_PUSH_FLOAT = 5;
        public const byte INST_PUSH_RET = 6;
        public const byte INST_PUSH_TEXT = 7;
        public const byte INST_PUSH_MESS = 8;
        public const byte INST_PUSH_GVAR = 9;
        public const byte INST_PUSH_LVAR = 10;
        public const byte INST_STORE_GVAR = 11;
        public const byte INST_STORE_LVAR = 12;
        public const byte INST_ENTER = 13;
        public const byte INST_LEAVE = 14;
        public const byte INST_JMP = 15;
        public const byte INST_JMPZ = 16;
        public const byte INST_CALL = 17;
        public const byte INST_RET = 18;
        public const byte INST_LOG_OR = 19;
        public const byte INST_LOG_AND = 20;
        public const byte INST_LOG_NOT = 21;
        public const byte INST_OR = 22;
        public const byte INST_XOR = 23;
        public const byte INST_AND = 24;
        public const byte INST_NOT = 25;
        public const byte INST_CMP_EQ = 26;
        public const byte INST_CMP_NE = 27;
        public const byte INST_CMP_LT = 28;
        public const byte INST_CMP_LE = 29;
        public const byte INST_CMP_GT = 30;
        public const byte INST_CMP_GE = 31;
        public const byte INST_SHL = 32;
        public const byte INST_SHR = 33;
        public const byte INST_ADD = 34;
        public const byte INST_SUB = 35;
        public const byte INST_MUL = 36;
        public const byte INST_DIV = 37;
        public const byte INST_MOD = 38;
        public const byte INST_NEG = 39;
        public const byte INST_NAME = 40;
        public const byte INST_TEXT = 41;
        public const byte INST_PAGE = 42;
        public const byte INST_OPTION = 43;
        public const byte INST_PROC = 44;
        public const byte INST_LINE = 45;

        public static string GetInstructionString(byte instruction, out int paramNum)
        {
            paramNum = instruction switch
            {
                INST_POP_N or INST_PUSH_INT or INST_PUSH_FLOAT or INST_PUSH_TEXT or INST_PUSH_MESS or INST_PUSH_GVAR or
                INST_PUSH_LVAR or INST_STORE_GVAR or INST_STORE_LVAR or INST_ENTER or INST_JMP or INST_JMPZ or INST_CALL or
                INST_NAME or INST_TEXT or INST_PROC or INST_LINE => 1,
                INST_OPTION => 2,
                _ => 0
            };

            return instruction switch
            {
                INST_POP => "INST_POP",
                INST_POP_N => "INST_POP_N",
                INST_POP_RET => "INST_POP_RET",
                INST_PUSH_INT => "INST_PUSH_INT",
                INST_PUSH_FLOAT => "INST_PUSH_FLOAT",
                INST_PUSH_RET => "INST_PUSH_RET",
                INST_PUSH_TEXT => "INST_PUSH_TEXT",
                INST_PUSH_MESS => "INST_PUSH_MESS",
                INST_PUSH_GVAR => "INST_PUSH_GVAR",
                INST_PUSH_LVAR => "INST_PUSH_LVAR",
                INST_STORE_GVAR => "INST_STORE_GVAR",
                INST_STORE_LVAR => "INST_STORE_LVAR",
                INST_ENTER => "INST_ENTER",
                INST_LEAVE => "INST_LEAVE",
                INST_JMP => "INST_JMP",
                INST_JMPZ => "INST_JMPZ",
                INST_CALL => "INST_CALL",
                INST_RET => "INST_RET",
                INST_LOG_OR => "INST_LOG_OR",
                INST_LOG_AND => "INST_LOG_AND",
                INST_LOG_NOT => "INST_LOG_NOT",
                INST_OR => "INST_OR",
                INST_XOR => "INST_XOR",
                INST_AND => "INST_AND",
                INST_NOT => "INST_NOT",
                INST_CMP_EQ => "INST_CMP_EQ",
                INST_CMP_NE => "INST_CMP_NE",
                INST_CMP_LT => "INST_CMP_LT",
                INST_CMP_LE => "INST_CMP_LE",
                INST_CMP_GT => "INST_CMP_GT",
                INST_CMP_GE => "INST_CMP_GE",
                INST_SHL => "INST_SHL",
                INST_SHR => "INST_SHR",
                INST_ADD => "INST_ADD",
                INST_SUB => "INST_SUB",
                INST_MUL => "INST_MUL",
                INST_DIV => "INST_DIV",
                INST_MOD => "INST_MOD",
                INST_NEG => "INST_NEG",
                INST_NAME => "INST_NAME",
                INST_TEXT => "INST_TEXT",
                INST_PAGE => "INST_PAGE",
                INST_OPTION => "INST_OPTION",
                INST_PROC => "INST_PROC",
                INST_LINE => "INST_LINE",
                _ => "UNKNOWN INSTRUCTION"
            };
        }

        public static object TyperHelper(byte instruction, byte[] code, int i)
        {
            return instruction switch
            {
                INST_POP_N or INST_PUSH_TEXT or INST_PUSH_MESS or INST_PUSH_LVAR or INST_STORE_GVAR or INST_STORE_LVAR or INST_ENTER or INST_JMP or INST_JMPZ or INST_CALL or INST_TEXT or INST_OPTION or INST_PROC => BitConverter.ToUInt32(code, i),
                INST_PUSH_FLOAT => BitConverter.ToSingle(code, i),
                _ => (object)BitConverter.ToInt32(code, i),
            };
        }

        public static string SetCommandStr(Command c, ScriptFile sf, ScriptMessage? sm, ref int messIndex)
        {
            //__cdecl
            switch (c.Instruction)
            {
                case INST_POP:
                    {
                        Mark(sf, 1);
                        return "Pop a value";
                    }

                case INST_POP_N:
                    {
                        Mark(sf, BitConverter.ToUInt32(c.Parameter));
                        return $"Pop multiple values";
                    }
                case INST_POP_RET:
                    return $"Pop a return value";
                case INST_PUSH_INT:
                    return $"Push an integer value";
                case INST_PUSH_FLOAT:
                    return $"Push a floating-point value";
                case INST_PUSH_RET:
                    return $"Push the return value";
                case INST_PUSH_TEXT://并非所有的TEXT都会使用
                    return $"Push a string: {sf.TextString[BitConverter.ToUInt32(c.Parameter)]}";
                case INST_PUSH_MESS://并非所有的MESS都会使用
                    {
                        messIndex++;
                        return $"{((sm == null) ? "意外的指令,此表无Mess" : sm.DataString[messIndex - 1])}";
                    }
                case INST_PUSH_GVAR:
                    return $"Push a global variable";
                case INST_PUSH_LVAR:
                    return $"Push a local variable";
                case INST_STORE_GVAR:
                    return $"Assign to a global variable";
                case INST_STORE_LVAR:
                    return $"Assign to a local variable";
                case INST_ENTER:
                    return $"Function start";
                case INST_LEAVE:
                    return $"Function end";
                case INST_JMP:
                    return $"Jump";
                case INST_JMPZ:
                    return $"Conditional jump";
                case INST_CALL:
                    return $"Call function offset: {BitConverter.ToUInt32(c.Parameter) + 1}";
                case INST_RET:
                    return $"Return";
                case INST_LOG_OR:
                    return $"Logical OR";
                case INST_LOG_AND:
                    return $"Logical AND";
                case INST_LOG_NOT:
                    return $"Logical NOT";
                case INST_OR:
                    return $"Bitwise OR";
                case INST_XOR:
                    return $"Bitwise XOR";
                case INST_AND:
                    return $"Bitwise AND";
                case INST_NOT:
                    return $"Bitwise NOT";
                case INST_CMP_EQ:
                    return $"Comparison(equal)";
                case INST_CMP_NE:
                    return $"Comparison(not equal)";
                case INST_CMP_LT:
                    return $"Comparison(less than)";
                case INST_CMP_LE:
                    return $"Comparison(less than or equal)";
                case INST_CMP_GT:
                    return $"Comparison(greater than)";
                case INST_CMP_GE:
                    return $"Comparison(greater than or equal)";
                case INST_SHL:
                    return $"Left bitwise shift";
                case INST_SHR:
                    return $"Right bitwise shift";
                case INST_ADD:
                    return $"Add";
                case INST_SUB:
                    return $"Sub";
                case INST_MUL:
                    return $"Multiplication";
                case INST_DIV:
                    return $"Division";
                case INST_MOD:
                    return $"Mod(remainder)";
                case INST_NEG:
                    return $"Negation(sign reversal)";
                case INST_NAME:
                    return $"Character name";
                case INST_PAGE:
                    return $"Message page break";
                case INST_OPTION:
                    return (sm == null) ? "意外的指令，此表无Mess" : $"option({sm.DataString[BitConverter.ToUInt32(c.Parameter)]})";

                    //messIndex++;
                    //return (sm == null) ? "意外的指令，此表无Mess" : $"option({sm.DataString[messIndex - 1]})";
                case INST_LINE:
                    return $"File line number";
                case INST_PROC:
                    uint index = BitConverter.ToUInt32(c.Parameter);
                    string funcName = index switch
                    {
                        30 => "(Voice)",
                        37 => "(Choices)",
                        _ => "",
                    };
                    string note="";
                    if (index == 30)
                    {
                        Command param2 = sf.GetCommandFromEnd(2);
                        if (param2.Instruction == INST_PUSH_INT)
                            note = $"({BitConverter.ToUInt16(param2.Parameter)})";
                    }

                    //if(index>= ProcNames.Length)
                    //    return $"Execute unknown built-in function";
                    return $"Execute built-in function {index}{funcName}{note}";
                case INST_TEXT:
                    return (sm == null) ? "意外的指令，此表无Mess" : sm.DataString[BitConverter.ToUInt32(c.Parameter)];

                    //messIndex++;
                    //return (sm == null) ? "意外的指令，此表无Mess" : sm.DataString[messIndex - 1];
                default:
                    return "UNKNOWN";
            }
        }

        private static void Mark(ScriptFile sf, uint j)
        {
            int k = sf.Commands.Count - 1;
            for (int i = 0; i < j; i++)
            {
                if (sf.Commands[k].Instruction <= 10 && sf.Commands[k].Instruction >= 4 && !sf.Commands[k].IsProcSet)
                {
                    sf.Commands[k--].IsProcSet = true;
                }
            }
        }
    }
}
