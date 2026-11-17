# RevisaAi

Aplicativo mobile para controle de manutenção de veículos.
Trabalho Final da disciplina de Desenvolvimento Mobile (URI - Campus de Erechim).

## Tecnologias
- .NET 10 / .NET MAUI (Android e Windows)
- SQLite (pacote `sqlite-net-pcl`)
- Arquitetura MVVM

## Visual
- Tema "Painel", inspirado no painel de instrumentos do carro, com modo claro e escuro (escolha em Ajustes e sobre).
- Fontes: Barlow, Barlow Condensed e IBM Plex Mono (licença SIL OFL, em `Resources/Fonts`).

## Estrutura do projeto
| Pasta | Conteúdo |
|---|---|
| `Model` | Classes das tabelas: `CVeiculo`, `CManutencao`, `CAbastecimento`, `COutroGasto`, `CLembrete` |
| `DAO` | Acesso ao banco: `CBanco` (conexão) e um DAO por tabela, todos herdando de `CDAO<T>` |
| `ModelView` | ViewModels com as propriedades e comandos usados pelas telas |
| `View` | Telas em XAML |
| `Util` | `CMensagem` (Toast / Snackbar) |

## Como executar
1. Abrir `RevisaAi.slnx` no Visual Studio 2026 (com a carga de trabalho .NET MAUI instalada).
2. Aguardar a restauração dos pacotes NuGet.
3. Escolher o destino **Windows Machine** ou um emulador/dispositivo Android.
4. Pressionar F5.

O banco de dados (`revisaai.db3`) é criado automaticamente na primeira execução.

## Como rodar no celular Android
**Pelo cabo USB (para testar e apresentar):**
1. No celular: Configurações > Sobre o telefone > tocar 7 vezes em "Número da versão".
2. Em Opções do desenvolvedor, ativar "Depuração USB".
3. Conectar o cabo, aceitar o aviso no celular e escolher o aparelho em "Android Local Devices" no Visual Studio.
4. Pressionar F5.

**Gerando um APK (para instalar sem o Visual Studio):**
1. Dar dois cliques em `gerar-apk.bat`.
2. Copiar para o celular o arquivo `-Signed.apk` da pasta `RevisaAi\bin\Release\net10.0-android\publish`.
3. Abrir o arquivo no celular e permitir a instalação de fontes desconhecidas.
