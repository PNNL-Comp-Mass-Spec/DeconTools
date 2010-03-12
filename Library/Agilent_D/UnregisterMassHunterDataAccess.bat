set REGASMPROG=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\regasm.exe

%REGASMPROG% BaseCommon.dll /tlb /u
%REGASMPROG% BaseDataAccess.dll /tlb /u
%REGASMPROG% MassSpecDataReader.dll /tlb /u