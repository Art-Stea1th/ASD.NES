# ASD.NES
### Emulator of the "Nintendo Entertainment System" game console.
|![Battle City Screenshot](https://github.com/Art-Stea1th/ASD.NES/blob/master/Screenshots/battle-city.jpg)|![Battle City Screenshot](https://github.com/Art-Stea1th/ASD.NES/blob/master/Screenshots/sky-destroyer.jpg)|![Battle City Screenshot](https://github.com/Art-Stea1th/ASD.NES/blob/master/Screenshots/pacman.jpg)|
|:---:|:---:|:---:|

#### Capabilities:
- Almost all code - implemented as a cross-platform library .NET Standard 1.1.
- View (WPF): so far only for a single platform (Windows 10 or 7 with the .NET Framework 4.5).
- There are sound, channels: 2 Square, 1 Triangular, but the channels "Noise" and "Delta Modulation" so far aren't impl.
- Supports all games on the "Mapper 0" (NROM).
- You can play together, but the ability to reassign keys is not yet available.

| Joypad Button    | Player One        | Player Two         |
| :---             |      :---:        |       :---:        |
| **Left**         | *A*               | *Left*             |
| **Up**           | *W*               | *Up*               |
| **Right**        | *D*               | *Right*            |
| **Down**         | *S*               | *Down*             |
| **Select**       | **R-Shift**       | **R-Shift**        |
| **Start**        | **Enter**         | **Enter**          |
| **B**            | *K*               | *Insert (Num "0")* |
| **A**            | *L*               | *Delete (Num ",")* |
