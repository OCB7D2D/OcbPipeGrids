@echo off

call MC7D2D PipeGrids.dll /reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" ^
-recurse:Sources/*.cs -recurse:Harmony/*.cs && ^
echo Successfully compiled PipeGrids.dll

REM Library\*.cs Utils\*.cs PipeBlocks\*.cs PipeGridManager\*.cs PlantManager\*.cs && ^

pause