# Licensing

Quantum Forge Unity Package uses a split licensing model:

## C# Source Code (MIT)

All C# source code in this package is licensed under the
[MIT License](LICENSE.md). You can read it, modify it, use it in your games,
contribute to it, and redistribute it freely.

## Native Plugins (Proprietary)

The compiled native libraries (.bundle, .dll, .so) in the Plugins directories
are proprietary. See the LICENSE.md files in each Plugins subdirectory for the
full terms. The short version:

- **Free** for applications generating under $100K/year in revenue
- **Attribution** required: "Powered by Quantum Forge"
- **No reverse engineering** of the compiled binaries
- Above $100K, contact hello@quantum.dev for a commercial license

## Why This Split?

The C# source code is the API surface you build with. We want it open so you
can read, debug, learn from, and contribute to it. The compiled quantum
simulator is years of specialized C++ work and is how we sustain the project.
This model lets us keep Quantum Forge free for indie developers and students
while building a sustainable business.

## Trademarks

"Quantum Forge" and the Quantum Native logo are trademarks of Quantum Realm
Games, LLC. The MIT license grants rights to the code, not to the trademarks.
