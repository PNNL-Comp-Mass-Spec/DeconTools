set REGASMPROG=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\regasm.exe

%REGASMPROG% BaseCommon.dll /tlb /u
%REGASMPROG% BaseDataAccess.dll /tlb /u
%REGASMPROG% MassSpecDataReader.dll /tlb /u

%REGASMPROG% BaseCommon.dll /tlb /codebase
%REGASMPROG% BaseDataAccess.dll /tlb	/codebase
%REGASMPROG% MassSpecDataReader.dll /tlb	/codebase

