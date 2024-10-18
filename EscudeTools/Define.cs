//以下是垃圾代码，闲人勿入
namespace EscudeTools
{
    public static class Define
    {
        public static readonly string[] ProcNames =
        [
                "proc_end", "proc_call", "proc_argv", "proc_argc", "proc_typeof", "proc_int", "proc_float", "proc_abs",
                "proc_rand", "proc_min", "proc_max", "proc_rgb", "proc_refer", "proc_credit", "proc_log_new", "proc_log_out",
                "proc_title", "proc_auto_save", "proc_is_pass", "proc_event", "proc_scene", "proc_open_name", "proc_notice",
                "proc_log_img", "proc_msg_opt", "proc_cf", "proc_cv", "proc_vt", "proc_frame", "proc_text", "proc_vals",
                "proc_clear", "proc_gap", "proc_menu_opt", "proc_menu", "proc_wait", "proc_lsf_init", "proc_lsf_set",
                "proc_lsf_get", "proc_lsf_break", "proc_dt", "proc_ps", "proc_cg", "proc_cg_org", "proc_cg_set", "proc_cg_get",
                "proc_cg_em", "proc_cg_clr", "proc_cg_disp", "proc_path", "proc_tween", "proc_trans", "proc_mot_set",
                "proc_mot_get", "proc_quake", "proc_flash", "proc_flt", "proc_ptcl", "proc_sync", "proc_auto_kill", "proc_movie",
                "proc_bgm_play", "proc_bgm_stop", "proc_bgm_vol", "proc_bgm_fx", "proc_amb_play", "proc_amb_stop", "proc_amb_vol",
                "proc_amb_fx", "proc_se_play", "proc_se_stop", "proc_se_wait", "proc_se_vol", "proc_se_fx", "proc_voc_play",
                "proc_voc_stop", "proc_voc_wait", "proc_voc_vol", "proc_voc_fx", "proc_bgv_play", "proc_bgv_stop", "proc_bgv_vol",
                "proc_bgv_fx", "proc_set_param", "proc_get_param", "proc_jump", "proc_date", "proc_flow", "proc_diary",
                "proc_unlock", "proc_section", "proc_omake"
            ];
        // 说句实话，我觉得这些定义可能会发生变化
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

        public static string SetCommandStr(Command c, ScriptFile sf, ScriptMessage sm, ref int messIndex)
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
                case INST_PUSH_TEXT:
                    return $"Push a string: {sf.TextString[BitConverter.ToUInt32(c.Parameter)]}";
                case INST_PUSH_MESS:
                    {
                        messIndex++;
                        return $"{sm.DataString[messIndex - 1]}";
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
                    return $"Set menu option";
                case INST_LINE:
                    return $"File line number";
                case INST_PROC:
                    uint index = BitConverter.ToUInt32(c.Parameter);
                    return $"Execute built-in function: {ProcNames[index]} {SetExtStr(c, sf)}";
                case INST_TEXT:
                    messIndex++;
                    return sm.DataString[messIndex - 1];
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

        private static string SetExtStr(Command c, ScriptFile sf)
        {
            switch (BitConverter.ToUInt32(c.Parameter))
            {
                case 0:
                    {
                        //(0 = 标题 / -1 = 游戏结束，保留数据 / 1以上 = 通关，保留数据)
                        string[] ps = ["结束代码"];
                        SetExtStr1(ps, sf);
                        return "脚本结束";
                    }
                case 1:
                    {
                        string[] ps = ["脚本编号"];
                        SetExtStr1(ps, sf);
                        return "调用文件";
                    }
                case 2:
                    {
                        string[] ps = ["参数索引"];
                        SetExtStr1(ps, sf);
                        return "获取函数帧的参数";
                    }
                case 3:
                    {
                        string[] ps = ["参数数量"];
                        SetExtStr1(ps, sf);
                        return "获取函数帧的参数数量";
                    }
                case 4:
                    {
                        string[] ps = ["值"];
                        SetExtStr1(ps, sf);
                        return "获取变量的类型";
                    }
                case 5:
                    {
                        string[] ps = ["值"];
                        SetExtStr1(ps, sf);
                        return "转换为整数";
                    }
                case 6:
                    {
                        string[] ps = ["值"];
                        SetExtStr1(ps, sf);
                        return "转换为浮点数";
                    }
                case 7:
                    {
                        string[] ps = ["值"];
                        SetExtStr1(ps, sf);
                        return "绝对值";
                    }
                case 8:
                    {
                        string[] ps = ["可变长度参数"];
                        SetExtStr1(ps, sf);
                        return "获取随机数";
                    }
                case 9:
                    {
                        string[] ps = ["可变长度参数"];
                        SetExtStr1(ps, sf);
                        return "获取最小值";
                    }
                case 10:
                    {
                        string[] ps = ["可变长度参数"];
                        SetExtStr1(ps, sf);
                        return "获取最大值";
                    }
                case 11:
                    {
                        string[] ps = ["R", "G", "B", "A"];
                        SetExtStr1(ps, sf);
                        return "将RGB值转换为颜色代码";
                    }
                case 12:
                    {
                        string[] ps = ["索引", "可变长度参数"];
                        SetExtStr1(ps, sf);
                        return "引用转换";
                    }
                case 13:
                    {
                        string[] ps = ["编号"];
                        SetExtStr1(ps, sf);
                        return "员工名单滚动";
                    }
                case 14:
                    {
                        string[] ps = ["文件名"];
                        SetExtStr1(ps, sf);
                        return "新建日志";
                    }
                case 15:
                    {
                        string[] ps = ["文件名", "输出值..."];
                        SetExtStr1(ps, sf);
                        return "输出日志";
                    }
                case 16:
                    {
                        string[] ps = ["标题字符串"];
                        SetExtStr1(ps, sf);
                        return "设置场景标题";
                    }
                case 17:
                    {
                        return "自动保存";
                    }
                case 18:
                    {
                        string[] ps = ["脚本编号"];
                        SetExtStr1(ps, sf);
                        return "获取脚本通过标志";
                    }
                case 19:
                    {
                        string[] ps = ["CG编号"];
                        SetExtStr1(ps, sf);
                        return "设置CG欣赏标志";
                    }
                case 20:
                    {
                        string[] ps = ["脚本编号"];
                        SetExtStr1(ps, sf);
                        return "设置场景回想标志";
                    }
                case 21:
                    {
                        string[] ps = ["名称编号"];
                        SetExtStr1(ps, sf);
                        return "打开名称";
                    }
                case 22:
                    {
                        string[] ps = ["文本", "显示时间", "强制标志"];
                        SetExtStr1(ps, sf);
                        return "显示通知文本";
                    }
                case 23:
                    {
                        string[] ps = ["角色ID"];
                        SetExtStr1(ps, sf);
                        return "设置日志中的角色图像";
                    }
                case 24:
                    {
                        string[] ps = ["文本控制ID", "设置标志"];
                        SetExtStr1(ps, sf);
                        return "文本控制标志的设置";
                    }
                case 25:
                    {
                        string[] ps = ["图像ID", "表情ID"];
                        SetExtStr1(ps, sf);
                        return "表情指定";
                    }
                case 26:
                    {
                        string[] ps = ["声音编号"];
                        SetExtStr1(ps, sf);
                        return "声音指定";
                    }
                case 27:
                    {
                        string[] ps = ["时间", "音轨"];
                        SetExtStr1(ps, sf);
                        return "声音的播放时间等待";
                    }
                case 28:
                    {
                        string[] ps = ["框架编号"];
                        SetExtStr1(ps, sf);
                        return "文本框架的更改";
                    }
                case 29:
                    {
                        //显示标志 (0 = 隐藏 / 1 = 显示)
                        //显示时间 (1 / 1000毫秒)
                        string[] ps = ["显示标志", "显示时间"];
                        SetExtStr1(ps, sf);
                        return "文本框架的显示/隐藏";
                    }
                case 30:
                    {
                        string[] ps = ["可变长参数"];
                        SetExtStr1(ps, sf);
                        return "文本引用值的设置";
                    }
                case 31:
                    {
                        string[] ps = ["时间(1/1000 ms)"];
                        SetExtStr1(ps, sf);
                        return "文本消息的消去";
                    }
                case 32:
                    {
                        string[] ps = ["x间隔", "y间隔"];
                        SetExtStr1(ps, sf);
                        return "文本间距的设置";
                    }
                case 33:
                    {
                        string[] ps = ["选择允许标志"];
                        SetExtStr1(ps, sf);
                        return "菜单选项的设置";
                    }
                case 34:
                    {
                        string[] ps = ["随机显示标志", "流程分支标志"];
                        SetExtStr1(ps, sf);
                        return "菜单选择";
                    }
                case 35:
                    {
                        string[] ps = ["延迟时间 (以毫秒为单位)", "点击取消标志"];
                        SetExtStr1(ps, sf);
                        return "延迟处理";
                    }
                case 36:
                    {
                        string[] ps = ["角色ID"];
                        SetExtStr1(ps, sf);
                        return "初始化LSF部件";
                    }
                case 37:
                    {
                        string[] ps = ["角色ID", "部件状态..."];
                        SetExtStr1(ps, sf);
                        return "设置LSF部件状态";
                    }
                case 38:
                    {
                        string[] ps = ["角色ID", "部件ID"];
                        SetExtStr1(ps, sf);
                        return "获取LSF部件状态";
                    }
                case 39:
                    {
                        string[] ps = ["角色ID", "部件ID..."];
                        SetExtStr1(ps, sf);
                        return "LSF部件破坏效果";
                    }
                case 40:
                    {
                        string[] ps = ["相对值"];
                        SetExtStr1(ps, sf);
                        return "相对指定显示对象属性";
                    }
                case 41:
                    {
                        string[] ps = ["显示对象..."];
                        SetExtStr1(ps, sf);
                        return "指定显示对象列表";
                    }
                case 42:
                    {
                        string[] ps = ["显示对象", "图像ID", "表情ID", "分辨率等级", "滤镜ID", "Z", "X", "Y", "水平缩放率", "垂直缩放率", "旋转角度", "不透明度"];
                        SetExtStr1(ps, sf);
                        return "设置显示对象";
                    }
                case 43:
                    {
                        string[] ps = ["显示对象", "X", "Y"];
                        SetExtStr1(ps, sf);
                        return "设置显示对象的原点坐标";
                    }
                case 44:
                    {
                        string[] ps = ["显示对象", "属性ID", "属性值"];
                        SetExtStr1(ps, sf);
                        return "设置显示对象的属性值";
                    }
                case 45:
                    {
                        string[] ps = ["显示对象", "属性ID"];
                        SetExtStr1(ps, sf);
                        return "获取显示对象的属性值";
                    }
                case 46:
                    {
                        string[] ps = ["显示对象", "效果编号", "部位ID", "相对于部位的X坐标", "相对于部位的Y坐标", "显示顺序"];
                        SetExtStr1(ps, sf);
                        return "设置情感效果";
                    }
                case 47:
                    {
                        string[] ps = ["显示对象列表"];
                        SetExtStr1(ps, sf);
                        return "删除显示对象";
                    }
                case 48:
                    {
                        string[] ps = ["效果编号", "显示时间 (ms)", "时间曲线", "翻转规则图像标志", "逆向播放标志", "颜色"];
                        SetExtStr1(ps, sf);
                        return "更新屏幕";
                    }
                case 49:
                    {
                        string[] ps = ["显示对象", "时间", "X", "Y", "水平缩放率", "垂直缩放率", "旋转角度", "不透明度"];
                        SetExtStr1(ps, sf);
                        return "设置插值";
                    }
                case 50:
                    {
                        string[] ps = ["显示对象", "插值动画ID"];
                        SetExtStr1(ps, sf);
                        return "插值动画";
                    }
                case 51:
                    {
                        return "插值";
                    }
                case 52:
                    {
                        string[] ps = ["显示对象", "变量编号", "值"];
                        SetExtStr1(ps, sf);
                        return "设置MOT的变量";
                    }
                case 53:
                    {
                        string[] ps = ["显示对象", "变量编号"];
                        SetExtStr1(ps, sf);
                        return "获取MOT的变量";
                    }
                case 54:
                    {
                        string[] ps = ["显示对象", "X轴震动像素数", "Y轴震动像素数", "效果时间"];
                        SetExtStr1(ps, sf);
                        return "屏幕震动";
                    }
                case 55:
                    {
                        string[] ps = ["闪光颜色", "闪光次数"];
                        SetExtStr1(ps, sf);
                        return "闪光效果";
                    }
                case 56:
                    {
                        string[] ps = ["滤镜ID", "模式", "颜色", "水平模糊", "垂直模糊"];
                        SetExtStr1(ps, sf);
                        return "设置颜色滤镜";
                    }
                case 57:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置特殊效果";
                    }
                case 58:
                    {
                        string[] ps = ["效果标志", "取消标志"];
                        SetExtStr1(ps, sf);
                        return "屏幕效果等待";
                    }
                case 59:
                    {
                        string[] ps = ["显示对象", "自动消失标志"];
                        SetExtStr1(ps, sf);
                        return "设置自动消失标志";
                    }
                case 60:
                    {
                        string[] ps = ["视频编号"];
                        SetExtStr1(ps, sf);
                        return "播放视频";
                    }
                case 61:
                    {
                        string[] ps = ["BGM编号", "淡入时间", "音量", "播放开始时间"];
                        SetExtStr1(ps, sf);
                        return "播放BGM";
                    }
                case 62:
                    {
                        string[] ps = ["淡出时间"];
                        SetExtStr1(ps, sf);
                        return "停止BGM";
                    }
                case 63:
                    {
                        string[] ps = ["音量值", "淡入淡出时间"];
                        SetExtStr1(ps, sf);
                        return "设置BGM音量";
                    }
                case 64:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置BGM效果";
                    }
                case 65:
                    {
                        string[] ps = ["环境音编号", "淡入时间", "音量", "播放开始时间"];
                        SetExtStr1(ps, sf);
                        return "播放环境音";
                    }
                case 66:
                    {
                        string[] ps = ["淡出时间"];
                        SetExtStr1(ps, sf);
                        return "停止环境音";
                    }
                case 67:
                    {
                        string[] ps = ["音量值", "淡入淡出时间"];
                        SetExtStr1(ps, sf);
                        return "设置环境音音量";
                    }
                case 68:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置环境音效果";
                    }
                case 69:
                    {
                        string[] ps = ["轨道", "音效编号", "循环标志", "淡入时间", "音量", "播放开始时间"];
                        SetExtStr1(ps, sf);
                        return "播放音效";
                    }
                case 70:
                    {
                        string[] ps = ["轨道", "淡出时间"];
                        SetExtStr1(ps, sf);
                        return "停止音效";
                    }
                case 71:
                    {
                        string[] ps = ["轨道"];
                        SetExtStr1(ps, sf);
                        return "音效等待";
                    }
                case 72:
                    {
                        string[] ps = ["轨道", "音量值", "淡入淡出时间"];
                        SetExtStr1(ps, sf);
                        return "设置音效音量";
                    }
                case 73:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置音效效果";
                    }
                case 74:
                    {
                        string[] ps = ["轨道", "语音编号", "淡入时间", "音量", "播放开始时间"];
                        SetExtStr1(ps, sf);
                        return "播放角色语音";
                    }
                case 75:
                    {
                        string[] ps = ["轨道", "淡出时间"];
                        SetExtStr1(ps, sf);
                        return "停止角色语音";
                    }
                case 76:
                    {
                        string[] ps = ["轨道"];
                        SetExtStr1(ps, sf);
                        return "角色语音等待";
                    }
                case 77:
                    {
                        string[] ps = ["轨道", "音量值", "淡入淡出时间"];
                        SetExtStr1(ps, sf);
                        return "设置角色语音音量";
                    }
                case 78:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置角色语音效果";
                    }
                case 79:
                    {
                        string[] ps = ["轨道", "语音编号", "淡入时间", "音量", "播放开始时间"];
                        SetExtStr1(ps, sf);
                        return "播放BGV";
                    }
                case 80:
                    {
                        string[] ps = ["轨道", "淡出时间"];
                        SetExtStr1(ps, sf);
                        return "停止BGV";
                    }
                case 81:
                    {
                        string[] ps = ["轨道", "音量值", "淡入淡出时间"];
                        SetExtStr1(ps, sf);
                        return "设置BGV音量";
                    }
                case 82:
                    {
                        string[] ps = ["效果编号"];
                        SetExtStr1(ps, sf);
                        return "设置BGV效果";
                    }
                case 83:
                    {
                        string[] ps = ["类别ID", "参数ID", "参数值"];
                        SetExtStr1(ps, sf);
                        return "设置参数";
                    }
                case 84:
                    {
                        string[] ps = ["类别ID", "参数ID"];
                        SetExtStr1(ps, sf);
                        return "获取参数";
                    }
                case 85:
                    {
                        string[] ps = ["脚本ID"];
                        SetExtStr1(ps, sf);
                        return "文件跳转";
                    }
                case 86:
                    {
                        string[] ps = ["经过天数"];
                        SetExtStr1(ps, sf);
                        return "更新日期";
                    }
                case 87:
                    {
                        string[] ps = ["脚本ID"];
                        SetExtStr1(ps, sf);
                        return "更新并显示流程图";
                    }
                case 88:
                    {
                        string[] ps = ["日记ID", "附加标志", "等待标志"];
                        SetExtStr1(ps, sf);
                        return "更新并显示日记";
                    }
                case 89:
                    {
                        string[] ps = ["脚本ID", "父脚本ID"];
                        SetExtStr1(ps, sf);
                        return "解锁区块";
                    }
                case 90:
                    {
                        string[] ps = ["章节ID"];
                        SetExtStr1(ps, sf);
                        return "设置章节";
                    }
                case 91:
                    {
                        return "附加内容";
                    }
            }
            return "";
        }

        private static void SetExtStr1(string[] ps, ScriptFile sf)
        {
            int i = sf.Commands.Count - 2;
            sf.Commands[i + 1].IsProcSet = true;
            for (int k = 0; k < ps.Length; k++)
            {
                if (sf.Commands[i].IsProcSet || sf.Commands[i].Instruction < 4 || sf.Commands[i].Instruction > 10)
                {
                    k--;
                    i--;
                    continue;
                }

                sf.Commands[i].Helper += $": {ps[k]}";
                sf.Commands[i].IsProcSet = true;
                i--;
            }
        }
    }
}
