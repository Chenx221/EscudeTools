namespace EscudeTools
{
    public static class Define
    {
        public const int SCR_LINE_MAX = 4096;

        public const byte INST_POP = 1;
        public const byte INST_POP_N = 2;
        public const byte INST_POP_ACC = 3;
        public const byte INST_PUSH = 4;
        public const byte INST_PUSH_ACC = 5;
        public const byte INST_PUSH_TEXT = 6;
        public const byte INST_PUSH_MESS = 7;
        public const byte INST_PUSH_GVAR = 8;
        public const byte INST_PUSH_LVAR = 9;
        public const byte INST_STORE_GVAR = 10;
        public const byte INST_STORE_LVAR = 11;
        public const byte INST_ENTER = 12;
        public const byte INST_LEAVE = 13;
        public const byte INST_JMP = 14;
        public const byte INST_JMPZ = 15;
        public const byte INST_CALL = 16;
        public const byte INST_RET = 17;
        public const byte INST_LOG_OR = 18;
        public const byte INST_LOG_AND = 19;
        public const byte INST_LOG_NOT = 20;
        public const byte INST_OR = 21;
        public const byte INST_XOR = 22;
        public const byte INST_AND = 23;
        public const byte INST_NOT = 24;
        public const byte INST_CMP_EQ = 25;
        public const byte INST_CMP_NE = 26;
        public const byte INST_CMP_LT = 27;
        public const byte INST_CMP_LE = 28;
        public const byte INST_CMP_GT = 29;
        public const byte INST_CMP_GE = 30;
        public const byte INST_SHL = 31;
        public const byte INST_SHR = 32;
        public const byte INST_ADD = 33;
        public const byte INST_SUB = 34;
        public const byte INST_MUL = 35;
        public const byte INST_DIV = 36;
        public const byte INST_MOD = 37;
        public const byte INST_NEG = 38;
        public const byte INST_NAME = 39;
        public const byte INST_TEXT = 40;
        public const byte INST_PAGE = 41;
        public const byte INST_OPTION = 42;
        public const byte INST_PROC = 43;
        public const byte INST_LINE = 44;

        public static string GetInstructionString(byte instruction, out int paramNum)
        {
            paramNum = instruction switch
            {
                INST_POP_N or INST_PUSH or INST_PUSH_TEXT or INST_PUSH_MESS or INST_PUSH_GVAR or
                INST_PUSH_LVAR or INST_STORE_GVAR or INST_STORE_LVAR or INST_ENTER or INST_JMP or
                INST_JMPZ or INST_CALL or INST_NAME or INST_TEXT or INST_PROC or INST_LINE => 1,
                INST_OPTION => 2,
                _ => 0
            };

            return instruction switch
            {
                INST_POP => "POP",
                INST_POP_N => "POP_N",
                INST_POP_ACC => "POP_ACC",
                INST_PUSH => "PUSH",
                INST_PUSH_ACC => "PUSH_ACC",
                INST_PUSH_TEXT => "PUSH_TEXT",
                INST_PUSH_MESS => "PUSH_MESS",
                INST_PUSH_GVAR => "PUSH_GVAR",
                INST_PUSH_LVAR => "PUSH_LVAR",
                INST_STORE_GVAR => "STORE_GVAR",
                INST_STORE_LVAR => "STORE_LVAR",
                INST_ENTER => "ENTER",
                INST_LEAVE => "LEAVE",
                INST_JMP => "JMP",
                INST_JMPZ => "JMPZ",
                INST_CALL => "CALL",
                INST_RET => "RET",
                INST_LOG_OR => "LOG_OR",
                INST_LOG_AND => "LOG_AND",
                INST_LOG_NOT => "LOG_NOT",
                INST_OR => "OR",
                INST_XOR => "XOR",
                INST_AND => "AND",
                INST_NOT => "NOT",
                INST_CMP_EQ => "CMP_EQ",
                INST_CMP_NE => "CMP_NE",
                INST_CMP_LT => "CMP_LT",
                INST_CMP_LE => "CMP_LE",
                INST_CMP_GT => "CMP_GT",
                INST_CMP_GE => "CMP_GE",
                INST_SHL => "SHL",
                INST_SHR => "SHR",
                INST_ADD => "ADD",
                INST_SUB => "SUB",
                INST_MUL => "MUL",
                INST_DIV => "DIV",
                INST_MOD => "MOD",
                INST_NEG => "NEG",
                INST_NAME => "NAME",
                INST_TEXT => "TEXT",
                INST_PAGE => "PAGE",
                INST_OPTION => "OPTION",
                INST_PROC => "PROC",
                INST_LINE => "LINE",
                _ => $"INST_{instruction:X2}"
            };
        }

        public static object TyperHelper(byte instruction, byte[] code, int i)
        {
            return BitConverter.ToInt32(code, i);
        }

        public static string SetCommandStr(Command c, ScriptFile sf, ScriptMessage sm)
        {
            uint param0 = 0;

            if (c.Parameter != null && c.Parameter.Length >= 4)
            {
                param0 = BitConverter.ToUInt32(c.Parameter, 0);
            }

            switch (c.Instruction)
            {
                case INST_PUSH_TEXT:
                    uint tIdx = param0 % SCR_LINE_MAX;
                    return $"PushString: {(tIdx < sf.TextString.Count() ? sf.TextString[(int)tIdx] : "OUT_OF_RANGE")}";

                case INST_PUSH_MESS:
                case INST_TEXT:
                    uint mIdx = param0 % SCR_LINE_MAX;
                    return (sm == null) ? $"MessID_{param0}" : $"Mess: {(mIdx < sm.DataString.Count() ? sm.DataString[(int)mIdx] : "OUT_OF_RANGE")}";

                case INST_CALL:
                    return $"Call Offset: 0x{param0:X8}";

                case INST_PROC:
                    return $"SysCall: Func_{param0}";

                case INST_LINE:
                    return $"-- Source Line: {param0} --";

                case INST_ENTER:
                    return $"Enter Frame (LocalVars: {param0})";

                case INST_JMP:
                    return $"Jump to 0x{param0:X8}";

                default:
                    int pCount;
                    string name = GetInstructionString(c.Instruction, out pCount);
                    return pCount > 0 ? $"{name} ({param0})" : name;
            }
        }
    }
}