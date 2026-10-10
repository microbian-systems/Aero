FROM mcr.microsoft.com/dotnet/sdk:10.0.401-noble@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317
RUN apt-get update && apt-get install -y --no-install-recommends \
    tesseract-ocr tesseract-ocr-eng libfontconfig1 fonts-dejavu-core \
    && apt-get clean
WORKDIR /work
COPY Directory.Packages.props ./
COPY src/Directory.Build.props ./src/
COPY src/Aero.Documents/ ./src/Aero.Documents/
COPY src/Aero.Documents.Ocr/ ./src/Aero.Documents.Ocr/
COPY src/Aero.Documents.PdfToImage/ ./src/Aero.Documents.PdfToImage/
COPY Tests/Aero.Documents.Ocr.Smoke/ ./tests/Aero.Documents.Ocr.Smoke/
RUN dotnet restore tests/Aero.Documents.Ocr.Smoke/Aero.Documents.Ocr.Smoke.csproj --locked-mode \
    && dotnet build tests/Aero.Documents.Ocr.Smoke/Aero.Documents.Ocr.Smoke.csproj -c Release --no-restore --nologo -warnaserror
RUN tesseract --version
ENTRYPOINT ["dotnet", "tests/Aero.Documents.Ocr.Smoke/bin/Release/net10.0/Aero.Documents.Ocr.Smoke.dll"]
