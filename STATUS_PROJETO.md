# Status do Projeto — CursinhoEnem

> Documento de continuidade. Cole isso como primeira mensagem de um chat novo (ou peça pra
> ler este arquivo) pra retomar o trabalho de onde parou.

## Contexto

Trabalho da faculdade (disciplina de Análise e Desenvolvimento de Sistemas). Começou como um
CRUD genérico de banco de questões/provas
(`GabrielBreda02/Sistema-Web-de-Banco-de-Questoes-e-Composicao-de-Provas`, com autenticação
JWT já adicionada). A orientadora — que também é "cliente" do projeto nessa disciplina — achou
o projeto genérico pouco diferenciado e sugeriu um pivô: uma **plataforma de simulados para
quem estuda pro ENEM** (cursinhos ou alunos por conta própria), com timer, redação, upload de
imagem nas questões e um acervo de questões reais do ENEM.

Decisão tomada: manter o repositório original intocado (representa a entrega já feita) e criar
um **repositório novo** para o produto evoluído.

- **Repositório atual/ativo:** https://github.com/GabrielBreda02/CursinhoEnem
- **Pasta local:** `../CursinhoEnem` (irmã da pasta do projeto original — mesma "Área de
  Trabalho")
- **Repositório antigo (não mexer mais):**
  `GabrielBreda02/Sistema-Web-de-Banco-de-Questoes-e-Composicao-de-Provas`

## Stack técnica

- **Back-end:** ASP.NET Core 8 (C#), Entity Framework Core + SQLite, em
  `fsg-banco-questoes-api/fsg-banco-questoes-api/` dentro do repo CursinhoEnem
- **Front-end:** HTML + CSS + JavaScript puro, sem framework e sem build step, na raiz do repo
- **Autenticação:** JWT (com claim de role Professor/Aluno) + hash de senha PBKDF2
- **Banco:** SQLite, criado automaticamente ao rodar a API (sem migrations, usa
  `EnsureCreated()`)

### Como rodar localmente

```bash
# API (porta 5000)
cd fsg-banco-questoes-api/fsg-banco-questoes-api
dotnet run --project BancoQuestoes.Api.csproj

# Front-end — servir a raiz do repo como arquivos estáticos, ex.:
python -m http.server 8080
# depois abrir http://localhost:8080/index.html
```

O `.csproj` já tem `<RollForward>LatestMajor</RollForward>`, então roda mesmo se só tiver um
.NET mais novo instalado (sem precisar do runtime 8.0 exato).

### Credenciais de teste (criadas automaticamente no primeiro run)

| Papel | E-mail | Senha |
|---|---|---|
| Professor | professor@teste.com | senha123 |
| Aluno | aluno@teste.com | senha123 |

Também já vem com 539 questões reais do ENEM (2015-2016 curadas + 2022-2024 em massa), 1 tema
de redação e 1 prova de exemplo montada (1 questão de cada área).

## O que já foi construído (todo o essencial está pronto e testado)

1. **Papéis Professor/Aluno** — `Usuario.Tipo`, claim de role no JWT,
   `[Authorize(Roles = "Professor")]` nos endpoints de escrita de questões/provas/temas
2. **Upload de imagem nas questões** — `POST /api/questoes/upload-imagem`, salva em
   `wwwroot/uploads/questoes/`, só Professor, valida tipo/tamanho
3. **Temas de Redação** — CRUD completo (`TemasRedacaoController`), associável a uma prova
4. **Fluxo do aluno fazendo prova** (`TentativasController`, tudo `[Authorize(Roles = "Aluno")]`):
   - `POST /api/tentativas/iniciar` — cria a tentativa, prazo (`ExpiraEm`) calculado **no
     servidor**, devolve as questões **sem revelar o gabarito**
   - `PUT /api/tentativas/{id}/respostas` — salva resposta por questão, valida dono/prazo/não
     finalizada
   - `POST /api/tentativas/{id}/finalizar` — salva redação, calcula nota
   - `GET /api/tentativas/{id}` — resultado (só depois de finalizada)
   - `GET /api/tentativas/minhas` — histórico
   - Telas: `SelecionarProva.html`, `FazerProva.html` (timer visual sincronizado com o prazo do
     servidor), `ResultadoProva.html`, `Historico.html`
5. **Acervo curado do ENEM** — `Data/Seed/questoes_enem.json`: 4 questões reais (uma por área,
   com ano/fonte citados) + 1 tema de redação oficial (ENEM 2016). `DbSeeder.cs` lê esse
   arquivo — pra adicionar mais conteúdo depois, é só incluir mais entradas no JSON seguindo o
   mesmo formato, sem mexer em código
6. **Repaginação visual completa** — design system em `Estilo.css` (paleta índigo/âmbar,
   tipografia Inter), navbar consistente em todas as páginas (`auth.js: renderNavbar()`), menu
   principal em cards, favicon
7. **Suíte de testes automatizados corrigida** — `BancoQuestoes.Tests` compila e os testes
   passam (`dotnet test BancoQuestoes.Tests.csproj`, ver observações técnicas abaixo; contagem
   atual em 45, ver item 17)
8. **Correção de redação pelo professor** — `TentativaProva.NotaRedacao`/`ComentarioRedacao`,
   `RedacoesController` (`api/redacoes`, `[Authorize(Roles = "Professor")]`), telas
   `Redacoes.html` (lista) e `CorrigirRedacao.html` (nota 0-1000 + comentário); o aluno vê a
   correção em `ResultadoProva.html` assim que o professor salva
9. **Retomar prova em andamento** — `POST /api/tentativas/iniciar` agora reaproveita a
   tentativa não finalizada do aluno pra aquela prova (se ainda dentro do prazo) em vez de criar
   outra; se o prazo já passou sem finalizar, fecha ela automaticamente antes de abrir uma nova.
   `Historico.html` ganhou botão "Continuar Prova" pra esse caso
10. **Acervo do ENEM ampliado pra 539 questões** — as 4 originais (curadas manualmente) + 535
    novas de 2022, 2023 e 2024 (180 cada, menos as anuladas oficialmente pelo INEP e 2 com dado
    corrompido na fonte), importadas do dataset aberto `maritaca-ai/enem` (Hugging Face, Apache
    2.0). 142 delas têm imagem (gráfico/mapa/charge/tirinha) — baixadas e comitadas em
    `Data/Seed/Imagens/`, servidas pela API na rota `/seed-images` (ver observações técnicas).
    `DbSeeder.SeedProvaExemplo` foi ajustado pra pegar só 1 questão por área na prova de exemplo,
    já que antes pegava a tabela inteira
11. **Campo `Disciplina` removido do sistema inteiro** (model, requests, responses, controllers,
    seeder e testes, back e front) — era redundante com `Area` pra 535 das 539 questões (ver
    §Observações abaixo). `Questao.html` também perdeu o campo `Assuntos` do formulário (a
    propriedade continua existindo no model/API pra quem já tinha assunto cadastrado, só não é
    mais preenchida manualmente). `Prova` ganhou um campo `Turma` (string opcional, depois
    virou `TurmaId` — ver item 14) no lugar — antes existia um input "Turma/Semestre" na tela
    que nem chegava a ser salvo, agora persiste de verdade
12. **Busca por palavra no enunciado + paginação** — `GET /api/questoes` agora aceita
    `busca` (contém no `Titulo`, substitui os antigos filtros por disciplina/assunto),
    `pagina` e `tamanhoPagina` (padrão 20), devolvendo um envelope
    `{ itens, paginaAtual, totalPaginas, totalItens }` (`Responses/PaginacaoResponse.cs`).
    `Questoes.html` (banco de questões) e `Prova.html` (montagem de prova) usam isso pra não
    depender mais de scroll infinito — botões de página no rodapé, com `criarControlesPaginacao()`
    compartilhado em `auth.js`. Os nomes das áreas do ENEM também foram encurtados na exibição
    (`formatArea()` em `auth.js` tira o "e suas Tecnologias") — o valor salvo no banco continua
    o nome oficial completo
13. **Filtro por área + confirmação visual na composição de prova** — `GET /api/questoes`
    ganhou o parâmetro `area`, combinável com `busca`. Ao adicionar uma questão à prova aparece
    um toast ("Questão adicionada à prova.") e o botão daquela questão vira "✓ Adicionada"
    (desabilitado), inclusive depois de refiltrar
14. **Turmas de verdade** — nova entidade `Turma` (`Models/Turma.cs`, `TurmasController`,
    `api/turmas`). Professor cria turmas, matricula/remove alunos (`Usuario.TurmaId`) e atribui
    uma prova a uma turma (`Prova.TurmaId`, substituindo o campo texto solto do item 11). Aluno
    só vê provas abertas (`TurmaId == null`) ou da própria turma — `GET /api/Provas` filtra por
    papel, e `POST /api/tentativas/iniciar` confere de novo no servidor (403 se a turma não
    bater), não é só esconder da lista. `DbSeeder` ganhou uma turma de exemplo com o aluno de
    teste já matriculado
15. **Correção de redação pelas 5 competências do ENEM** — `TentativaProva` trocou o campo
    `NotaRedacao` gravado por `NotaComp1`..`NotaComp5` (0-200 cada, múltiplos de 20, validado no
    controller) mais uma propriedade calculada `NotaRedacao` (soma das 5, `entity.Ignore` no
    `DbContext`, não é coluna). `CorrigirRedacao.html` mostra os 5 campos com total em tempo
    real; `ResultadoProva.html` mostra o detalhamento pro aluno, não só o total
16. **Sorteio automático de questões por área** — `GET /api/questoes/sortear?porArea=N` busca
    os IDs de cada uma das 4 áreas, embaralha em memória (não dá pra embaralhar via SQL/EF sem
    cair em avaliação client-side) e devolve N de cada. `Prova.html` soma o resultado à seleção
    atual com um toast de confirmação
17. **Testes automatizados de Turmas, sorteio e correção por competências** — 28 testes novos
    em 3 classes (`TurmasIntegrationTests`, `TentativasIntegrationTests`,
    `RedacoesIntegrationTests`), mais 4 em `QuestoesIntegrationTests` pro sorteio. Suíte total
    sobe de 17 pra 45. `IntegrationTestBase` ganhou um segundo `HttpClient` autenticado como
    Aluno (`_alunoClient` + `AlunoId`), necessário pra testar fluxos que só o papel Aluno faz
    (iniciar/finalizar prova, ler o próprio resultado). Escrever o teste de exclusão de turma
    pegou um bug real: `TurmasController.DeleteTurma` não tinha `.Include(Alunos)/.Include(Provas)`
    antes de remover, então o `SetNull` não zerava `TurmaId` em memória — funcionava por acaso
    no SQLite real (a constraint do banco cobria) mas não no provedor InMemory dos testes.
    Corrigido com os dois `.Include()`

Todas as fases foram testadas por `curl`/script Python e pelo navegador (fluxo completo dos
dois papéis) antes de cada commit.

## Observações técnicas (aprendidas corrigindo a suíte de testes)

- **`BancoQuestoes.Tests.csproj` e `BancoQuestoes.Api.csproj` ficam na mesma pasta e
  compartilham `bin`/`obj`.** Rodar `dotnet build BancoQuestoes.Api.csproj` sozinho e depois
  `dotnet test BancoQuestoes.Tests.csproj` deixa esses intermediários num estado que confunde o
  build do projeto de testes (some referência ao Xunit). Fluxo confiável: restaurar e testar o
  projeto de testes de uma vez só, sem builds separados do projeto da API no meio —
  `dotnet restore BancoQuestoes.Tests.csproj && dotnet test BancoQuestoes.Tests.csproj --no-restore`.
- **`testhost.exe` (do pacote de testes) não tem o `RollForward` que o `Api.csproj` tem** — se
  só houver um .NET mais novo instalado (sem o runtime 8.0 exato), rodar os testes precisa de
  `DOTNET_ROLL_FORWARD=LatestMajor` no ambiente:
  `DOTNET_ROLL_FORWARD=LatestMajor dotnet test BancoQuestoes.Tests.csproj`.
- **SQLite não guarda o `Kind` do `DateTime`.** Sem conversores explícitos, todo `DateTime` lido
  de volta do banco (fora do objeto recém-criado em memória) volta como `Unspecified`, o que
  fazia o JSON perder o sufixo `Z` e o navegador ler a data como horário local em vez de UTC —
  bug real encontrado testando "retomar prova" (o timer pulava ~3h). Corrigido com
  `ConfigureConventions` em `BancoQuestoesContext.cs` forçando `DateTimeKind.Utc` na leitura.
- **Mudar o schema (ex.: remover um campo do model) não migra o banco existente sozinho** —
  como o projeto usa `EnsureCreated()` em vez de `Migrate()`, é preciso apagar o `.db` local
  (`bancoquestoes_dev.db` em dev) pra ele recriar do zero com o schema novo; o `DbSeeder` então
  repopula tudo automaticamente na próxima subida da API. Foi assim que o campo `Disciplina`
  saiu do banco ao ser removido do model.

## Observações técnicas (aprendidas ampliando o acervo do ENEM)

- **`wwwroot/uploads/` é gitignored de propósito** (uploads de verdade feitos pelo professor não
  devem ir pro repo) — mas isso significa que imagens de **seed** não podem morar lá, ou ninguém
  mais consegue rodar o projeto com elas. Por isso as 142 imagens do acervo 2022-2024 ficam em
  `Data/Seed/Imagens/` (comitado, copiado pro build igual ao `questoes_enem.json`) e são servidas
  numa rota estática separada, `/seed-images` (configurada em `Program.cs`, ver
  `PhysicalFileProvider`). Se crescer o acervo de novo e usar `POST /api/questoes/upload-imagem`
  pra gerar as imagens, lembrar de mover o resultado pra cá antes de commitar — senão elas somem
  no próximo `git clone`.
- **Fonte dos dados 2022-2024**: dataset `maritaca-ai/enem` no Hugging Face (180 questões por
  ano, texto+alternativas+gabarito, licença Apache 2.0). A API pública `api.enem.dev` (que cobre
  mais anos) está atrás de proteção anti-bot e não respondeu a nenhuma tentativa de acesso
  automatizado — se um dia quiser tentar de novo esticar pra 2015-2021, essa é a barreira a
  vencer, não o Hugging Face.
- **Puxar dados grandes de API externa**: pedir pro `WebFetch` reproduzir JSON "verbatim" funciona
  pra lotes pequenos, mas o modelo por trás dele pode falhar silenciosamente perto do limite de
  saída dele (viu-se um registro com campo faltando e uma resposta com contagem de linhas errada
  em lotes de 30). O caminho confiável foi usar o navegador real (`javascript_tool`, que tem
  acesso à internet) pra fazer o `fetch` e devolver o JSON puro — sem modelo nenhum reescrevendo
  no meio do caminho — e ler o resultado persistido em disco via Python, nunca via
  print()/terminal (o console do Windows corrompe acento em UTF-8 na exibição, mesmo com o
  arquivo em disco intacto — sempre validar o arquivo real, não a tela).

## Pendências conhecidas / possíveis próximos passos

- **Algumas questões 2022-2024 têm formatação de tabela em markdown no enunciado** (ex.: dados
  de um experimento em formato `| coluna | coluna |`) — ficaram como texto puro com `|` literais
  em vez de reformatadas, porque não dava pra revisar as 535 questões uma a uma. Cosmético, o
  conteúdo continua correto e legível.
- **Uma questão só pode ter 1 imagem** (`Questao.ImagemUrl` é string única) — um punhado das
  questões 2022-2024 tinha 2 imagens na fonte (ex.: dois gráficos complementares); só a primeira
  foi importada. Não travou nenhuma questão, mas quem for revisar o acervo pode achar um caso ou
  outro em que falta contexto visual.
- **`Usuario.TurmaId` é uma FK simples** — um aluno só pode estar em uma turma por vez. Suficiente
  pro caso de uso atual, mas não modela aluno em duas turmas ao mesmo tempo (viraria N:N).
- Nenhuma dessas pendências bloqueia o uso do sistema — são possíveis evoluções futuras.
