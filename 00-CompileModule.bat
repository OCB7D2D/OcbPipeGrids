@echo off

call MC7D2D PipeGrids.dll /reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" ^
Harmony\*.cs Helpers\*.cs KdTreeLib\*.cs KdTreeLib\Math\*.cs Messages\*.cs NetPackages\*.cs PipeGridBlocks\*.cs ^
Interfaces\*.cs PipeGridNodes\*.cs PipeGridProcess\*.cs PipeGridStructs\*.cs Ticker\*.cs Utils\*.cs
echo Successfully compiled PipeGrids.dll

REM Library\*.cs Utils\*.cs PipeBlocks\*.cs PipeGridManager\*.cs PlantManager\*.cs && ^

pause