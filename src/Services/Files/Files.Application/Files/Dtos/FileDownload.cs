namespace Files.Application.Files.Dtos;

public sealed record FileDownload(Stream Content, string FileName, string ContentType);
