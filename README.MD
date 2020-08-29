To get breakpoints to hit:
- Install the C# extension version 1.22.0
- Install JS debugger (nightly) version 2020.6.617
- `cd Tests && dotnet watch build`
- `dotnet run` (in separate terminal)
- Trigger launch configuration 'Debug'

Tests
- Requirements: Node.js v12.x
- Tests can be run by running `dotnet test *Tests` or pressing F7
- Tests can be debuged by selecting them with a regex 
  - in launch.json -> Debug Test -> args for selection tests, or `*` for all

JS
- One-time:
  - install yarn (v1.22.4)
  - install npm (v6.14.5)
- `cd js/blatex && rm -r node_modules && yarn install && yarn build && cd .. && npm install && npm run build && cd ..`


Submodules:
  - One-time initialize katex-fonts submodule:
    - `git submodule add --branch "v0.10.0-rc.1" git@github.com:JeroenBos/KaTeX.git js/blatex`
    - `cd js/blatex && rmdir submodules/katex-fonts && git submodule add -f https://github.com/KaTeX/katex-fonts.git submodules/katex-fonts`
  - Update blatex submodule:
    - `git submodule foreach git pull`
    - commit the diff file

