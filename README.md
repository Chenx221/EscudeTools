# EscudeTools

### 已测试

悠刻のファムファタル (解封包正常、EV ST合成正常)

廃村少女 ～妖し惑ひの籠の郷～ (解封包正常，EV合成正常，ST不支持）

### 使用说明：

**1. 解包 ESC-ARC bin 文件：**
命令：`EscudeTools.exe -u <路径>`

- **-u**: 解包 ESC-ARC bin 文件
- 解包指定目录中的所有 ESC-ARC bin 文件。
- 解包后的内容将保存在 `output` 目录中。
- `*.json` 文件包含包信息；请勿删除（用于重新封包）。
- `lzwManifest.json` 文件包含 LZW 文件信息；如果您希望不使用 LZW 压缩重新封包，请删除此文件。

------

**2. 重新封包 ESC-ARC bin 文件：**
命令：`EscudeTools.exe -r (-c) <路径>`

- **-r**: 重新封包 ESC-ARC bin 文件
- **-r -c**: 重新封包 ESC-ARC bin 文件并使用现有的自定义密钥
- 重新封包指定目录中的所有文件为 ESC-ARC bin 格式。
- 可选的 **-c** 标志用于使用现有 ESC-ARC bin 中的自定义密钥。
- 默认密钥为 ...（请查看源代码以获取）。
- 有关输出目录中 JSON 文件的说明，请参考解包使用说明。

------

**3. 解包脚本 bin 文件：**
命令：`EscudeTools.exe -v <路径> -t <类型>`

- **-v -t**: 解包脚本 bin 文件及其类型
- 解包指定目录中的所有脚本 bin 文件到 SQLite 数据库中。
- 忽略 001 文件；程序将在需要时读取它们。
- 必须指定要解包的脚本 bin 文件类型。
- 支持以下类型：0、1、2
  - **类型 0**: 完整，这会创建包含所有 .bin 和 .001 信息的 `script.db`。
  - **类型 1**: 只导出 bin 中的文本，这会创建 `script_text.db` 以及大量 .dat 文件（非文本数据）。
  - **类型 2**: 只导出 001 中的文本，这会创建 `script_sm.db`，包含所有 .001 信息。

------

**4. 重新封包脚本 bin 文件：**
命令：`EscudeTools.exe -e <路径> -t <类型>`

- **-e -t**: 重新封包脚本 bin 文件及其类型
- 重新封包指定目录中的所有 SQLite 数据库文件为脚本 bin 文件。
- 必须指定要重新封包的脚本 bin 文件类型。
- 支持以下类型：0、1、2
  - **类型 0**: 完整，这会生成 .bin 和 .001 文件。
  - **类型 1**: 这会生成 .001 文件。
  - **类型 2**: 这会生成 .bin 文件。

------

**5. 解包 db_\*.bin 文件：**
命令：`EscudeTools.exe -d <路径>`

- **-d**: 解包 db_*.bin 文件到 SQLite
- 将路径下所有的 db_*.bin 文件导出到单独的 SQLite 数据库中。

------

**6. 重新封包 SQLite 数据库：**
命令：`EscudeTools.exe -f <路径>`

- **-f**: 重新封包 SQLite 到 db_*.bin
- 将路径下所有的 SQLite 数据库恢复为 db_*.bin 文件。

------

**7. 合成 EV 图像：**
命令：`EscudeTools.exe -c <EvPath> <db_graphics.db 路径>`

- **-c**: 合成 EV 图像
- 提供一组相同尺寸的 EV 图像和 `db_graphics.db` 来合成图像。

------

**8. 合成 ST 图像：**
命令：`EscudeTools.exe -s <StPath> <db_graphics.db 路径>`

- **-s**: 合成 ST 图像
- 提供一组相同尺寸的 ST 图像和 `db_graphics.db` 来合成图像。

------

**9. 打印帮助信息：**
命令：`EscudeTools.exe -h`

- **-h**: 打印帮助信息