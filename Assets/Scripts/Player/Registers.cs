using System;
using System.Collections.Generic;

public class Program {
    readonly ProgramCardData[] _registers;
    public IReadOnlyCollection<ProgramCardData> Cards => _registers;

    public Program(int registerCount) {
        _registers = new ProgramCardData[registerCount];
    }

    public ProgramCardData this[int index] {
        get => _registers[index];
    }

    public event Action<int, ProgramCardData, ProgramCardData> OnRegisterChanged;

    public void SetCard(int index, ProgramCardData card) {
        var oldCard = _registers[index];
        _registers[index] = card;
        OnRegisterChanged?.Invoke(index, oldCard, card);
    }
}