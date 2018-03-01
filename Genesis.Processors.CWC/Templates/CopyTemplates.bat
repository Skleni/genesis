chcp 1252

for /r "D:\Dokumente\Command & Conquer Generäle Stunde Null Data\Maps" %%f in (*smallflag*.map) do xcopy "%%f" "SmallFlag\" /Y
for /r "D:\Dokumente\Command & Conquer Generäle Stunde Null Data\Maps" %%f in (*largeflag*.map) do xcopy "%%f" "LargeFlag\" /Y
for /r "D:\Dokumente\Command & Conquer Generäle Stunde Null Data\Maps" %%f in (*smallfuel*.map) do xcopy "%%f" "SmallFuelDepot\" /Y
for /r "D:\Dokumente\Command & Conquer Generäle Stunde Null Data\Maps" %%f in (*largefuel*.map) do xcopy "%%f" "LargeFuelDepot\" /Y
for /r "D:\Dokumente\Command & Conquer Generäle Stunde Null Data\Maps" %%f in (*village*.map) do xcopy "%%f" "Village\" /Y