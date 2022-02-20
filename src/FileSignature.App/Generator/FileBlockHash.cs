namespace FileSignature.App.Generator;

public readonly record struct FileBlockHash(uint BlockIndex, ReadOnlyMemory<byte> Block);