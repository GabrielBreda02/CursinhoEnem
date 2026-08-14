# Guia do Sistema — CursinhoEnem

> Documento de estudo. Objetivo: você conseguir abrir qualquer arquivo do projeto, saber o que
> ele faz, por que ele existe e como ele se conecta com o resto — e apresentar isso com
> segurança.

---

## Sumário

1. [O que é o sistema e de onde ele veio](#1-o-que-é-o-sistema-e-de-onde-ele-veio)
2. [Tecnologias usadas e por quê](#2-tecnologias-usadas-e-por-quê)
3. [Como executar](#3-como-executar)
4. [Arquitetura em uma página](#4-arquitetura-em-uma-página)
5. [O caminho de uma requisição](#5-o-caminho-de-uma-requisição)
6. [Modelo de dados](#6-modelo-de-dados)
7. [Mapa de arquivos — o que cada um faz](#7-mapa-de-arquivos--o-que-cada-um-faz)
8. [Os fluxos explicados por dentro](#8-os-fluxos-explicados-por-dentro)
9. [Decisões técnicas e armadilhas resolvidas](#9-decisões-técnicas-e-armadilhas-resolvidas)
10. [Testes automatizados](#10-testes-automatizados)
11. [Segurança do sistema](#11-segurança-do-sistema)
12. [Trilha de estudo em 5 sessões](#12-trilha-de-estudo-em-5-sessões)
13. [Roteiro de demonstração](#13-roteiro-de-demonstração)
14. [Perguntas prováveis e como responder](#14-perguntas-prováveis-e-como-responder)
15. [Limitações conhecidas](#15-limitações-conhecidas)

---

## 1. O que é o sistema e de onde ele veio

### O ponto de partida

O projeto nasceu como um **Sistema Web de Banco de Questões e Composição de Provas**: um CRUD
genérico onde se cadastravam questões com alternativas e depois se montavam provas escolhendo
questões desse banco. Já tinha autenticação JWT, mas não tinha público-alvo definido — servia
para "qualquer prova de qualquer disciplina".

### O pivô

A avaliação foi que um CRUD genérico é pouco diferenciado. A direção adotada foi transformar o
sistema num **produto com público definido**: uma plataforma de simulados para quem estuda para
o ENEM — cursinhos que querem aplicar simulados aos alunos, ou alunos estudando por conta.

Isso mudou o sistema de "cadastrar dados" para "**executar um processo**". A diferença é o
coração da apresentação:

| Antes (banco de questões) | Agora (CursinhoEnem) |
|---|---|
| Cadastrar questão / montar prova | Aluno **faz** a prova dentro do sistema |
| Sem noção de tempo | Cronômetro regressivo controlado pelo servidor |
| Só múltipla escolha | Redação com tema e texto motivador |
| Só texto no enunciado | Upload de imagem (gráficos, mapas, charges, tirinhas) |
| Um tipo de usuário | Dois papéis: **Professor** e **Aluno**, com permissões diferentes |
| Correção manual | Correção automática das objetivas + correção humana da redação |
| Banco vazio | **539 questões reais do ENEM** já carregadas |

O repositório original foi mantido intocado (representa a entrega anterior) e este é um
repositório novo, com o produto evoluído.

### As 10 adições que compõem a evolução

Essa é a lista que vale decorar — cada item tem uma seção detalhada mais adiante.

1. **Papéis Professor/Aluno** — campo `Usuario.Tipo`, claim de role no JWT e
   `[Authorize(Roles = ...)]` protegendo cada endpoint.
2. **Upload de imagem nas questões** — `POST /api/questoes/upload-imagem`, com validação de
   tipo e tamanho, salvando em `wwwroot/uploads/questoes/`.
3. **Temas de Redação** — entidade + CRUD completo, associável a uma prova.
4. **Fluxo do aluno fazendo prova** — `TentativasController` com iniciar / responder /
   finalizar / consultar resultado / histórico, mais 4 telas novas.
5. **Acervo curado do ENEM** — arquivo `Data/Seed/questoes_enem.json` lido pelo `DbSeeder`,
   com questões reais citando ano e fonte.
6. **Repaginação visual** — design system em `Estilo.css`, navbar consistente, menu em cards,
   favicon.
7. **Suíte de testes corrigida** — 16 testes de integração compilando e passando.
8. **Correção de redação pelo professor** — `RedacoesController` + telas de lista e de
   correção (nota 0–1000 e comentário), visível ao aluno no resultado.
9. **Retomar prova em andamento** — atualizar a página não reinicia o cronômetro nem perde as
   respostas já dadas.
10. **Acervo ampliado para 539 questões** — 2022, 2023 e 2024 completos (menos as anuladas),
    142 delas com imagem versionada no repositório.

---

## 2. Tecnologias usadas e por quê

### Back-end

| Tecnologia | Papel no projeto | Por que essa escolha |
|---|---|---|
| **ASP.NET Core 8** (C#) | API REST que concentra toda a regra de negócio | Framework maduro, tipado, com injeção de dependência e autenticação embutidas |
| **Entity Framework Core 8** | ORM — traduz classes C# em tabelas e LINQ em SQL | Elimina SQL manual e mantém o modelo de dados no próprio código |
| **SQLite** | Banco de dados (arquivo `.db` local) | Zero instalação e zero configuração — o avaliador roda o projeto sem instalar servidor de banco |
| **JWT Bearer** | Autenticação sem sessão no servidor | O token carrega quem é o usuário e qual o papel dele; a API não guarda estado de login |
| **PBKDF2** (`Rfc2898DeriveBytes`) | Hash de senha | Algoritmo lento e com salt — inviabiliza rainbow tables e ataques de força bruta em massa |
| **Swagger / Swashbuckle** | Documentação interativa da API | Permite testar todos os endpoints pelo navegador, sem front-end |
| **Newtonsoft.Json** | Serialização JSON | Configurado com `ReferenceLoopHandling.Ignore` por causa das relações bidirecionais do EF |
| **xUnit + WebApplicationFactory** | Testes de integração | Sobe a API inteira em memória e testa por HTTP de verdade |

### Front-end

| Tecnologia | Papel |
|---|---|
| **HTML5** | Estrutura das 15 telas |
| **CSS3** | Design system próprio com variáveis CSS (`Estilo.css`) |
| **JavaScript puro (ES6+)** | Consumo da API via `fetch`, manipulação do DOM, cronômetro |
| **localStorage** | Guarda o token JWT e os dados de sessão no navegador |

**Sem framework e sem build step** — não há `npm install`, `node_modules`, webpack ou
transpilador. Abrir os arquivos num servidor estático já roda. É uma decisão defensável: o
front é simples, o foco do trabalho está no back-end, e reduz drasticamente o atrito para
qualquer pessoa executar o projeto.

---

## 3. Como executar

### Pré-requisito

.NET SDK instalado. O `.csproj` tem `<RollForward>LatestMajor</RollForward>`, então roda mesmo
se você só tiver uma versão mais nova que a 8.0.

### Passo 1 — subir a API (porta 5000)

```bash
cd fsg-banco-questoes-api/fsg-banco-questoes-api
dotnet run --project BancoQuestoes.Api.csproj
```

Na primeira execução isso cria o banco SQLite e popula automaticamente com usuários de teste,
o acervo do ENEM e uma prova de exemplo. O Swagger abre em `http://localhost:5000`.

### Passo 2 — servir o front-end (porta 8080)

Em **outro terminal**, na raiz do repositório:

```bash
python -m http.server 8080
```

Depois abra `http://localhost:8080/index.html`.

> **Por que precisa de um servidor?** Abrir o HTML com duplo clique usa o protocolo `file://`,
> e o navegador bloqueia requisições `fetch` a partir dele. Qualquer servidor estático resolve
> (Python, Live Server do VS Code, etc.).

### Credenciais de teste

| Papel | E-mail | Senha |
|---|---|---|
| Professor | `professor@teste.com` | `senha123` |
| Aluno | `aluno@teste.com` | `senha123` |

### Rodar os testes

```bash
cd fsg-banco-questoes-api/fsg-banco-questoes-api
dotnet restore BancoQuestoes.Tests.csproj
dotnet test BancoQuestoes.Tests.csproj --no-restore
```

Se sua máquina não tiver exatamente o runtime 8.0, defina `DOTNET_ROLL_FORWARD=LatestMajor`
antes de rodar (o `testhost` não herda o `RollForward` do `.csproj`).

### O que já vem no banco

- 2 usuários de teste (um de cada papel)
- **539 questões reais do ENEM** — 136 de Ciências Humanas, 135 de Linguagens, 134 de Ciências
  da Natureza e 134 de Matemática; **142 delas com imagem**
- 1 tema de redação oficial (ENEM 2016)
- 1 prova de exemplo montada com uma questão de cada área, 60 minutos, com redação associada

---

## 4. Arquitetura em uma página

```
┌─────────────────────────────────────────────────────────────┐
│  NAVEGADOR  (http://localhost:8080)                         │
│                                                             │
│  HTML (estrutura)  +  Estilo.css (visual)                   │
│  auth.js  ─ sessão, token, authFetch(), navbar              │
│  <Pagina>.js ─ um script por tela, chama a API via fetch    │
│  localStorage ─ guarda o token JWT                          │
└───────────────────────┬─────────────────────────────────────┘
                        │  HTTP + JSON
                        │  Header: Authorization: Bearer <token>
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  API ASP.NET CORE  (http://localhost:5000)                  │
│                                                             │
│  Program.cs ─ monta o pipeline: CORS → Autenticação →       │
│               Autorização → Controllers                     │
│                                                             │
│  Controllers/  ← recebem a requisição, validam, respondem   │
│      Auth · Questoes · Provas · TemasRedacao ·              │
│      Tentativas · Redacoes                                  │
│                                                             │
│  Requests/   ← formato de entrada  (+ validação por atributo)│
│  Responses/  ← formato de saída    (controla o que vaza)    │
│  Security/   ← JwtTokenService · PasswordHasher             │
│  Models/     ← as entidades do domínio                      │
│  Data/       ← BancoQuestoesContext (EF) · DbSeeder         │
└───────────────────────┬─────────────────────────────────────┘
                        │  Entity Framework Core
                        ▼
              ┌──────────────────────┐
              │  SQLite (arquivo)    │
              └──────────────────────┘
```

### As cinco camadas do back-end e o que cada uma resolve

1. **Models** — as entidades do domínio. É o que vira tabela no banco.
2. **Data** — o `DbContext`, que configura como as entidades viram tabelas, e o `DbSeeder`,
   que popula o banco inicial.
3. **Requests** — classes que descrevem o que o cliente **pode enviar**. Com atributos de
   validação (`[Required]`, `[Range]`, `[MinLength]`) verificados automaticamente.
4. **Responses** — classes que descrevem o que a API **devolve**. Existem para não retornar a
   entidade crua: é assim que a API esconde `SenhaHash` do usuário e esconde qual alternativa
   é a correta durante a prova.
5. **Controllers** — orquestram: recebem o Request, consultam o banco, aplicam a regra de
   negócio, montam o Response.

> **Ponto forte para citar na apresentação:** a separação Request/Response/Model é o que
> impede o vazamento de dado sensível. Se o controller devolvesse a entidade `Questao` direto,
> a resposta traria o campo `Correta` de cada alternativa — e o aluno veria o gabarito
> abrindo o DevTools durante a prova.

---

## 5. O caminho de uma requisição

Vale entender esse trajeto uma vez; todo o resto do sistema é repetição dele. Exemplo: o aluno
marca a alternativa B na questão 12.

1. **Evento no navegador** — o `change` do `<input type="radio">` dispara
   `salvarResposta(questaoId, alternativaId)` em `FazerProva.js`.
2. **`authFetch()`** (em `auth.js`) monta a requisição, lê o token do `localStorage` e
   acrescenta o header `Authorization: Bearer <token>`.
3. **Rede** — sai um `PUT http://localhost:5000/api/tentativas/7/respostas` com corpo JSON.
4. **Pipeline do ASP.NET Core** (`Program.cs`, nessa ordem):
   - `UseCors` — libera a chamada vinda da porta 8080 (origem diferente da API).
   - `UseAuthentication` — valida a assinatura do token, o emissor, o público e a validade;
     se ok, preenche o `User` da requisição com as claims.
   - `UseAuthorization` — confere o `[Authorize(Roles = "Aluno")]` do controller.
5. **Roteamento** — `[Route("api/tentativas")]` + `[HttpPut("{id:int}/respostas")]` levam ao
   método `Responder`.
6. **Model binding + validação** — o JSON vira um `ResponderTentativaRequest`; os atributos de
   validação rodam automaticamente.
7. **Regra de negócio** — o método confere: a tentativa existe? é deste aluno? já foi
   finalizada? o prazo expirou? a questão pertence à prova? a alternativa pertence à questão?
8. **Persistência** — o EF Core insere ou atualiza a linha em `RespostasAluno` e o
   `SaveChangesAsync()` executa o SQL.
9. **Resposta** — `200 OK` com `{ "message": "Resposta salva", "success": true }`.
10. **Volta ao navegador** — a promessa do `fetch` resolve; nesse caso o front não precisa
    fazer nada visualmente, o salvamento é silencioso.

---

## 6. Modelo de dados

### As sete entidades

```
Usuario                    TemaRedacao
  IdUsuario                  IdTemaRedacao
  Nome                       Titulo
  Email (único)              TextoMotivador
  SenhaHash                  Ano
  Tipo ("Professor"/"Aluno") Fonte
                                  │
                                  │ 0..1
                                  ▼
Questao  ◄──── N:N ────►  Prova
  IdQuestao                 IdProva
  Titulo (o enunciado)      Titulo
  Disciplina                Disciplina
  AssuntosJson              TempoLimiteMinutos
  Area (uma das 4 do ENEM)  TemaRedacaoId
  ImagemUrl
  Ano / Fonte
    │ 1:N
    ▼
Alternativa
  IdAlternativa
  Descricao
  Correta (bool)  ← nunca sai da API durante a prova
  QuestaoId


TentativaProva                     RespostaAluno
  IdTentativa                        IdResposta
  ProvaId  ──────► Prova             TentativaId ──► TentativaProva
  AlunoId  ──────► Usuario           QuestaoId   ──► Questao
  IniciadoEm                         AlternativaSelecionadaId ──► Alternativa
  ExpiraEm        ← prazo do servidor
  FinalizadoEm    ← nulo = em andamento
  TextoRedacao
  NotaObjetivas   ← calculada ao finalizar
  NotaRedacao     ← 0-1000, dada pelo professor
  ComentarioRedacao
```

### Relacionamentos e por que são assim

- **Questao ↔ Prova é muitos-para-muitos.** Uma questão pode aparecer em várias provas e uma
  prova tem várias questões. O EF cria automaticamente a tabela de junção `ProvaQuestao`
  (configurada em `BancoQuestoesContext.cs`). Isso é o que dá sentido a "banco de questões":
  a questão é reutilizável.
- **Questao → Alternativa é 1:N com `DeleteBehavior.Cascade`.** Apagar a questão apaga as
  alternativas dela — alternativa órfã não faz sentido.
- **TentativaProva → Prova/Usuario usa `DeleteBehavior.Restrict`.** O oposto do caso anterior:
  o banco **impede** apagar uma prova que já foi respondida por alguém, para não destruir
  histórico.
- **RespostaAluno tem índice único em (TentativaId, QuestaoId).** Garantia no nível do banco
  de que o aluno não tem duas respostas para a mesma questão na mesma tentativa. Quando ele
  troca de alternativa, o registro é atualizado, não duplicado.

### Dois detalhes de modelagem que rendem pergunta

**`AssuntosJson` + a propriedade `Assuntos`** — SQLite não tem tipo array. A solução foi
guardar a lista de assuntos como texto JSON na coluna `AssuntosJson` e expor uma propriedade
`Assuntos` em C# que serializa/desserializa na hora do acesso:

```csharp
public List<string> Assuntos
{
    get => JsonSerializer.Deserialize<List<string>>(AssuntosJson) ?? new();
    set => AssuntosJson = JsonSerializer.Serialize(value);
}
```

O `entity.Ignore(e => e.Assuntos)` no `DbContext` diz ao EF: "não tente criar coluna para
essa propriedade, ela é só uma visão da outra".

**`AreaConhecimento`** (`Models/AreaConhecimento.cs`) — uma classe estática com as 4 áreas do
ENEM como constantes e um `EhValida()`. Não é `enum` porque o valor guardado é o texto por
extenso; é validada no controller antes de gravar. Isso trava a área em um conjunto fechado,
que é o que permite filtrar o acervo por área com segurança.

---

## 7. Mapa de arquivos — o que cada um faz

### Back-end · `fsg-banco-questoes-api/fsg-banco-questoes-api/`

**Raiz**

| Arquivo | Função |
|---|---|
| `Program.cs` | Ponto de entrada. Registra serviços (EF, JWT, Swagger, CORS) e monta o pipeline HTTP na ordem correta. Também cria o banco e chama o seed. |
| `appsettings.json` | Configuração: string de conexão do SQLite e parâmetros do JWT (chave, emissor, público, validade de 120 min). |
| `BancoQuestoes.Api.csproj` | Projeto da API: pacotes NuGet e a regra que copia `Data/Seed/**` para a pasta de build. |
| `BancoQuestoes.Tests.csproj` | Projeto de testes, na mesma pasta, compilando **apenas** `Tests/**`. |

**`Models/`** — o domínio

| Arquivo | Função |
|---|---|
| `Usuario.cs` | Pessoa que usa o sistema. `Tipo` define o papel. |
| `Questao.cs` | Uma questão: enunciado (`Titulo`), disciplina, área do ENEM, assuntos, imagem opcional, ano e fonte. |
| `Alternativa.cs` | Uma opção de resposta, com o booleano `Correta`. |
| `Prova.cs` | Um simulado: título, disciplina, tempo limite e tema de redação opcional. |
| `TemaRedacao.cs` | Título + texto motivador (+ ano/fonte, para citar temas oficiais). |
| `TentativaProva.cs` | O registro de um aluno fazendo uma prova. É a entidade central do produto. |
| `RespostaAluno.cs` | Uma resposta dentro de uma tentativa. |
| `AreaConhecimento.cs` | As 4 áreas do ENEM + validação. |

**`Data/`**

| Arquivo | Função |
|---|---|
| `BancoQuestoesContext.cs` | Configura todo o mapeamento objeto-relacional: chaves, tamanhos, índices, relacionamentos e comportamento de exclusão. Também força `DateTimeKind.Utc` na leitura (ver §9). |
| `DbSeeder.cs` | Popula o banco na primeira execução: usuários de teste, acervo do ENEM (lido do JSON) e prova de exemplo. Cada etapa checa antes se já existe, então é seguro rodar sempre. |
| `Seed/questoes_enem.json` | O acervo: 539 questões + 1 tema de redação, em JSON. **Adicionar conteúdo aqui não exige mexer em código.** |
| `Seed/Imagens/` | As 142 imagens de questões do acervo, versionadas no repositório. |

**`Controllers/`** — a API propriamente dita

| Arquivo | Rota | Proteção |
|---|---|---|
| `AuthController.cs` | `api/auth` | Pública (registrar e login) |
| `QuestoesController.cs` | `api/questoes` | Leitura pública; escrita e upload só Professor |
| `ProvasController.cs` | `api/Provas` | Leitura pública; escrita só Professor |
| `TemasRedacaoController.cs` | `api/temas-redacao` | Leitura pública; escrita só Professor |
| `TentativasController.cs` | `api/tentativas` | Controller inteiro **só Aluno** |
| `RedacoesController.cs` | `api/redacoes` | Controller inteiro **só Professor** |

**`Security/`**

| Arquivo | Função |
|---|---|
| `JwtTokenService.cs` | Gera o token assinado com as claims `sub` (id), `email`, `name`, `jti` e **`role`** (o tipo do usuário). |
| `PasswordHasher.cs` | `Hash()` e `Verificar()` com PBKDF2-SHA256, salt aleatório de 16 bytes e 100.000 iterações. |

**`Tests/`** — 16 testes de integração (detalhes em §10).

### Front-end · raiz do repositório

**Compartilhados**

| Arquivo | Função |
|---|---|
| `auth.js` | O núcleo do front. Define `API_BASE`, salva/lê a sessão no `localStorage`, expõe `ehProfessor()`/`ehAluno()`, o `authFetch()` (que injeta o token e trata 401) e o `renderNavbar()`. **Carregado por todas as páginas.** |
| `Estilo.css` | Design system: paleta em variáveis CSS (índigo `#4f46e5` + âmbar), tipografia Inter, e os componentes (`.btn`, `.question-card`, `.navbar`, `.menu-card`, `.timer-badge`, `.badge-area`). |
| `favicon.svg` | Ícone da aba. |
| `index.html` | Menu principal. Um único arquivo que **monta o menu conforme o papel** de quem está logado: professor vê 4 cards, aluno vê 2, visitante vê entrar/cadastrar. |

**Autenticação**

| Arquivos | Função |
|---|---|
| `Login.html` / `Login.js` | Envia credenciais para `/api/auth/login`, salva a sessão e redireciona. |
| `Registro.html` / `Registro.js` | Cadastro com escolha de papel; valida senha e confirmação antes de enviar. |

**Telas do Professor**

| Arquivos | Função |
|---|---|
| `Questoes.html` / `Questoes.js` | Lista o banco de questões em cards, com badge de área, imagem e ações de editar/excluir. |
| `Questao.html` / `Questao.js` | Formulário de cadastro/edição. Mesma tela para os dois casos: se a URL tem `?id=`, carrega e faz `PUT`; se não, faz `POST`. Também faz o upload da imagem. |
| `Provas.html` / `Provas.js` | Lista as provas montadas. |
| `Prova.html` / `Prova.js` | Composição da prova: busca questões no banco com filtro, adiciona/remove da seleção, define tempo limite e tema de redação. |
| `TemasRedacao.html` / `TemasRedacao.js` | Lista os temas. |
| `TemaRedacao.html` / `TemaRedacao.js` | Cadastro/edição de tema com texto motivador. |
| `Redacoes.html` / `Redacoes.js` | Lista as redações entregues, marcando pendente ou corrigida. |
| `CorrigirRedacao.html` / `CorrigirRedacao.js` | Mostra tema + texto do aluno e recebe nota (0–1000) e comentário. |

**Telas do Aluno**

| Arquivos | Função |
|---|---|
| `SelecionarProva.html` / `SelecionarProva.js` | Lista os simulados disponíveis com quantidade de questões, tempo e tema. Confirma antes de iniciar. |
| `FazerProva.html` / `FazerProva.js` | A tela mais importante: inicia a tentativa, monta as questões, mantém o cronômetro, salva cada resposta e finaliza. |
| `ResultadoProva.html` / `ResultadoProva.js` | Placar, gabarito comentado questão a questão e o bloco da redação com a nota do professor. |
| `Historico.html` / `Historico.js` | Lista as tentativas anteriores; se alguma está em andamento, oferece "Continuar Prova". |

---

## 8. Os fluxos explicados por dentro

### 8.1 Cadastro e login

**Cadastro** (`AuthController.Registrar`):
1. Normaliza o e-mail (`Trim().ToLowerInvariant()`) — evita `Joao@X.com` e `joao@x.com` como
   contas diferentes.
2. Verifica se o e-mail já existe (além do índice único no banco, que é a garantia final).
3. Gera o hash da senha com PBKDF2 e grava. **A senha em texto nunca é armazenada.**

**Login** (`AuthController.Login`):
1. Busca o usuário pelo e-mail normalizado.
2. `PasswordHasher.Verificar(senha, hashArmazenado)` — recalcula o hash com o mesmo salt e
   compara em **tempo constante** (`CryptographicOperations.FixedTimeEquals`), o que evita
   ataques de temporização.
3. Se bate, `JwtTokenService.GerarToken(usuario)` devolve um token válido por 120 minutos.
4. A resposta traz token, nome, e-mail e tipo — o front guarda tudo no `localStorage`.

**Como o hash é guardado.** O formato é `iterações.salt.hash`, tudo em Base64 numa string só:

```
100000.aB3xK9...==.9fE2mQ...==
```

Guardar o número de iterações junto permite aumentar o custo no futuro sem invalidar as senhas
antigas — o `Verificar()` usa o valor que está gravado em cada registro.

### 8.2 Autorização por papel

O token carrega `new Claim(ClaimTypes.Role, usuario.Tipo)`. Com isso:

```csharp
[Authorize(Roles = "Professor")]   // em cada método de escrita de Questoes/Provas/Temas
[Authorize(Roles = "Aluno")]       // no TentativasController inteiro
[Authorize(Roles = "Professor")]   // no RedacoesController inteiro
```

O ASP.NET Core compara a claim `role` do token com o valor do atributo antes mesmo de o método
executar. Consequências que valem demonstrar:

- Um aluno chamando `POST /api/questoes` recebe **403 Forbidden**, mesmo logado.
- Um professor chamando `POST /api/tentativas/iniciar` também recebe 403.
- O front esconder o botão é conveniência, **não** é a segurança. A segurança está no servidor.

E dentro do `TentativasController` há uma segunda camada: não basta ser Aluno, tem que ser **o
dono da tentativa**:

```csharp
if (tentativa.AlunoId != alunoId) return Forbid();
```

Sem isso, o aluno A poderia ler o resultado do aluno B só trocando o número na URL.

### 8.3 Banco de questões e upload de imagem

`POST /api/questoes/upload-imagem` recebe o arquivo como `IFormFile` e valida:

- arquivo presente e não vazio;
- tamanho máximo de 5 MB;
- extensão em `.jpg`, `.jpeg`, `.png` ou `.webp`.

O arquivo é salvo com nome `Guid.NewGuid()` + extensão. **Por que renomear?** Duas razões: o
nome original poderia colidir com outro upload, e nomes vindos do cliente são uma via clássica
de ataque de travessia de diretório (`../../algo`). Gerando o nome no servidor, isso é
eliminado.

A resposta devolve só a URL relativa (`/uploads/questoes/<guid>.png`), que o front guarda no
campo `ImagemUrl` da questão. O fluxo na tela é em duas etapas: o upload acontece no momento
em que o professor escolhe o arquivo, e a URL retornada é enviada junto quando ele salva a
questão.

### 8.4 Composição da prova

`Prova.js` mantém um array `questoesSelecionadas` com os IDs escolhidos. O professor busca
questões com filtro por disciplina/assunto, clica em "Adicionar", e ao salvar o front envia:

```json
{
  "titulo": "Simulado 1",
  "disciplina": "Geral",
  "tempoLimiteMinutos": 180,
  "temaRedacaoId": 1,
  "QuestoesIds": [12, 47, 103]
}
```

O `ProvasController` valida no servidor:
- Todos os IDs de questão existem? (senão, retorna quais faltaram)
- O tema de redação informado existe?
- `TempoLimiteMinutos` está entre 1 e 600? (atributo `[Range]` no Request)

Só então cria a prova, e o EF grava as linhas da tabela de junção sozinho.

### 8.5 O fluxo do aluno fazendo prova — o coração do sistema

Esse é o fluxo que diferencia o produto. Vale saber os quatro passos de cor.

#### Passo 1 — `POST /api/tentativas/iniciar`

O front envia apenas `{ provaId }`. O servidor:

1. Carrega a prova com questões, alternativas e tema de redação.
2. Recusa se a prova não existe (404) ou não tem questões (400).
3. **Procura uma tentativa não finalizada** do mesmo aluno nessa prova (ver §8.6).
4. Se não houver, cria a tentativa e — o ponto crucial — **calcula o prazo no servidor**:
   ```csharp
   var iniciadoEm = DateTime.UtcNow;
   tentativa = new TentativaProva {
       IniciadoEm = iniciadoEm,
       ExpiraEm  = iniciadoEm.AddMinutes(prova.TempoLimiteMinutos)
   };
   ```
5. Monta a resposta usando `QuestaoProvaResponse` e `AlternativaProvaResponse` — classes que
   **não têm o campo `Correta`**. O gabarito simplesmente não trafega.

#### Passo 2 — `PUT /api/tentativas/{id}/respostas`

Chamado a cada clique numa alternativa, de forma silenciosa (não há botão "salvar
respostas"). Antes de gravar, o servidor checa cinco coisas: a tentativa existe, é deste
aluno, ainda não foi finalizada, o prazo não expirou, e a questão/alternativa realmente
pertencem à prova.

Grava com "insere ou atualiza": se já existe resposta para aquela questão, troca a
alternativa; se não, cria. O índice único no banco reforça a regra.

#### Passo 3 — `POST /api/tentativas/{id}/finalizar`

Recebe o texto da redação, marca `FinalizadoEm = DateTime.UtcNow` e calcula a nota:

```csharp
tentativa.Respostas.Count(r => r.AlternativaSelecionada?.Correta == true);
```

A correção das objetivas é **automática e feita no servidor** — o navegador nunca teve a
informação necessária para calcular isso.

#### Passo 4 — `GET /api/tentativas/{id}`

Só responde se a tentativa estiver finalizada (senão, 400) e se for do aluno logado (senão,
403). **Agora sim** a resposta usa `AlternativaResultadoResponse`, que inclui `Correta`. O
gabarito só existe depois que a prova acabou.

#### O cronômetro (`FazerProva.js`)

```javascript
expiraEm = new Date(dados.expiraEm);      // veio do servidor
intervalId = setInterval(atualizarTimer, 1000);
```

`atualizarTimer()` calcula `expiraEm - agora`, formata em `HH:MM:SS`, e nos últimos 5 minutos
adiciona a classe `timer-alerta` (destaque visual). Ao chegar a zero, avisa e chama
`finalizarProva(true)` — envio automático.

> **A pergunta clássica: "e se o aluno mexer no relógio do computador?"**
> Não adianta. O relógio do navegador só controla a **exibição** da contagem. O prazo real é o
> `ExpiraEm` gravado no banco, e todo `PUT` de resposta é comparado com `DateTime.UtcNow` **do
> servidor**. Se o prazo passou, a API recusa a resposta com "O tempo dessa prova já se
> esgotou" — independentemente do que o cronômetro na tela estiver mostrando.

### 8.6 Retomar prova em andamento

Problema real: o aluno atualiza a página no meio da prova. Sem tratamento, seria criada uma
tentativa nova, com o cronômetro reiniciado e as respostas anteriores abandonadas.

A solução está no `Iniciar`:

```csharp
var tentativaAberta = /* tentativa desse aluno, nessa prova, com FinalizadoEm == null */;

if (tentativaAberta != null && DateTime.UtcNow <= tentativaAberta.ExpiraEm)
{
    tentativa = tentativaAberta;             // retoma: mesmo prazo, mesmas respostas
}
else
{
    if (tentativaAberta != null)             // existia, mas o prazo já passou
    {
        tentativaAberta.FinalizadoEm = tentativaAberta.ExpiraEm;
        tentativaAberta.NotaObjetivas = CalcularNotaObjetivas(tentativaAberta);
    }
    /* cria uma tentativa nova */
}
```

Três comportamentos num bloco só:
- **Dentro do prazo** → retoma, e a resposta traz `RespostasSalvas` para o front remarcar os
  radios já escolhidos.
- **Prazo vencido sem finalizar** (o aluno fechou a aba e sumiu) → o servidor fecha a tentativa
  órfã com a nota que ela já tinha, datada no momento em que o prazo expirou — não no momento
  em que ele voltou. Depois abre uma nova.
- **Nenhuma tentativa aberta** → cria normalmente.

No `Historico.html`, tentativas sem `finalizadoEm` aparecem com o selo "Em andamento" e um
botão **Continuar Prova**.

### 8.7 Correção da redação

O ciclo completo entre os dois papéis:

1. O aluno escreve no `<textarea>` e finaliza — o texto vai em `TentativaProva.TextoRedacao`.
2. No resultado, ele vê "Aguardando correção do professor".
3. O professor abre **Corrigir Redações**. `GET /api/redacoes` lista as tentativas
   finalizadas **cuja prova tem tema de redação**, ordenadas da mais recente, marcando cada
   uma como pendente ou corrigida.
4. Ao abrir uma, `GET /api/redacoes/{id}` traz o tema, o texto motivador e o texto do aluno
   lado a lado.
5. `PUT /api/redacoes/{id}` grava `NotaRedacao` (validada de 0 a 1000 pelo `[Range]`) e
   `ComentarioRedacao`.
6. O aluno recarrega o resultado e vê a nota e o comentário — porque
   `ResultadoTentativaResponse` já carrega esses dois campos.

Note a divisão conceitual: **objetiva o sistema corrige; redação um humano corrige.** É a
divisão correta, e mostra que o sistema modela o processo real de um cursinho.

### 8.8 O acervo do ENEM e o seed

`DbSeeder.Seed()` roda a cada inicialização, mas cada etapa começa com um "já existe?" — então
não duplica nada.

- `SeedUsuarios` — cria os dois usuários de teste se a tabela estiver vazia.
- `SeedEnem` — lê `Data/Seed/questoes_enem.json` e insere temas e questões.
- `SeedProvaExemplo` — monta uma prova com **uma questão de cada área** (agrupa por `Area` e
  pega a primeira de cada grupo), 60 minutos, com o tema de redação associado.

O acervo tem 539 questões: 4 curadas manualmente (ENEM 2014–2016, com fonte citada) e 535
importadas das provas de 2022, 2023 e 2024 — 180 por ano, menos as anuladas oficialmente pelo
INEP e duas com dado corrompido na fonte. As 142 imagens ficam em `Data/Seed/Imagens/`.

**Extensibilidade:** acrescentar questões é editar o JSON, sem tocar em uma linha de C#. Esse é
um argumento de projeto que vale mencionar na apresentação.

---

## 9. Decisões técnicas e armadilhas resolvidas

Esta seção é ouro numa apresentação: são problemas reais encontrados e resolvidos, não teoria.

### 9.1 O bug do fuso horário no cronômetro

**Sintoma:** ao retomar uma prova, o cronômetro pulava cerca de 3 horas.

**Causa:** o SQLite não guarda o `Kind` do `DateTime`. Tudo que era lido de volta do banco
voltava como `Unspecified`; o serializador então omitia o sufixo `Z` do JSON; e o navegador
interpretava `"2026-08-14T18:00:00"` como horário **local** em vez de UTC.

**Correção** (`BancoQuestoesContext.cs`): um conversor de valor aplicado a todas as
propriedades `DateTime` do modelo, forçando `DateTimeKind.Utc` na leitura.

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder cb)
{
    cb.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    cb.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
}
```

É seguro porque o sistema inteiro só grava `DateTime.UtcNow` — nunca horário local.

### 9.2 Imagens do acervo fora da pasta de uploads

`wwwroot/uploads/` está no `.gitignore` de propósito: conteúdo enviado por professores não
deve ir para o repositório. Mas as imagens do **acervo** precisam ir, senão quem clona o
projeto fica com 142 questões quebradas.

Solução: as imagens do seed moram em `Data/Seed/Imagens/` (versionado, copiado no build) e são
servidas numa rota estática separada, configurada no `Program.cs`:

```csharp
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "Imagens")),
    RequestPath = "/seed-images"
});
```

Duas rotas de arquivo estático convivendo: `/uploads/...` para conteúdo gerado e
`/seed-images/...` para conteúdo versionado.

### 9.3 `WebRootPath` declarado explicitamente

O projeto começou como API pura, sem `wwwroot`. Sem declarar `WebRootPath = "wwwroot"` na
criação do builder, o provedor de arquivos estáticos é montado como nulo e o
`app.UseStaticFiles()` nunca encontra os uploads — mesmo criando a pasta depois.

### 9.4 Os dois `.csproj` na mesma pasta

`BancoQuestoes.Api.csproj` e `BancoQuestoes.Tests.csproj` compartilham a pasta, então o SDK
tentaria compilar os mesmos arquivos duas vezes. Cada um tem uma regra de exclusão:

- a API remove `Tests/**`;
- o de testes remove tudo (`**\*.cs`) e inclui só `Tests/**\*.cs`, recebendo os tipos da API
  pelo `ProjectReference`.

Como eles também compartilham `bin`/`obj`, compilar a API sozinha e depois rodar os testes
deixa os intermediários num estado inconsistente. O fluxo confiável é
`dotnet restore` + `dotnet test` do projeto de testes, sem build separado da API no meio.

### 9.5 O seed desligado durante os testes

Os testes sobem a aplicação real. Se o seed rodasse ali, os testes que verificam "lista vazia"
falhariam. Por isso o `Program.cs` tem:

```csharp
if (!app.Environment.IsEnvironment("Testing")) DbSeeder.Seed(context);
```

e a base de testes chama `builder.UseEnvironment("Testing")`.

### 9.6 `public partial class Program { }`

Com top-level statements, a classe `Program` gerada é interna e o `WebApplicationFactory<Program>`
não a enxerga. A declaração parcial pública no fim do arquivo resolve.

### 9.7 CORS

O front (porta 8080) e a API (porta 5000) são **origens diferentes** para o navegador. Sem a
política CORS registrada no `Program.cs`, todo `fetch` seria bloqueado. A política atual é
permissiva (`AllowAnyOrigin`), apropriada para desenvolvimento; em produção seria restrita ao
domínio real do front.

---

## 10. Testes automatizados

**16 testes de integração** em xUnit — 8 para questões e 8 para provas.

Não são testes unitários com mocks: o `WebApplicationFactory<Program>` **sobe a aplicação
inteira** e os testes conversam com ela por HTTP real, com o banco trocado por um provedor
**InMemory** com nome único por classe de teste (`Guid.NewGuid()`), garantindo isolamento.

`IntegrationTestBase` resolve dois detalhes:

- **Login antes dos testes** — como os endpoints de escrita exigem papel Professor, a base
  registra um professor e faz login no `InitializeAsync()`, guardando o token no header padrão
  do `HttpClient`. (Vai em `IAsyncLifetime` porque xUnit não aceita construtor assíncrono.)
- **Limpeza** — `Dispose()` apaga o banco em memória ao fim.

O que os testes cobrem: listar vazio, criar com dados válidos, rejeitar questão sem alternativa
correta, rejeitar dados inválidos, buscar por ID inexistente, atualizar, excluir, e prova com
questão inexistente.

---

## 11. Segurança do sistema

Resumo para citar de forma organizada:

| Ameaça | Defesa implementada |
|---|---|
| Vazamento de senha em caso de invasão do banco | PBKDF2-SHA256, salt aleatório por usuário, 100.000 iterações |
| Ataque de temporização na comparação de senha | `CryptographicOperations.FixedTimeEquals` |
| Usuário sem permissão executando ação de outro papel | `[Authorize(Roles = ...)]` validado no servidor |
| Aluno vendo o gabarito durante a prova | Responses específicas sem o campo `Correta` |
| Aluno lendo a tentativa de outro aluno | Verificação `tentativa.AlunoId != alunoId` → 403 |
| Aluno burlando o cronômetro | Prazo calculado e verificado no servidor (`ExpiraEm` vs `DateTime.UtcNow`) |
| Upload malicioso | Extensão restrita a 4 formatos, limite de 5 MB, nome gerado por GUID no servidor |
| Token adulterado ou expirado | Assinatura HMAC-SHA256 validada, mais emissor, público e validade |
| Sessão expirada em uso | `authFetch` intercepta 401, limpa o `localStorage` e redireciona ao login |
| Dados inválidos chegando ao banco | Atributos de validação nos Requests + validações de negócio nos controllers |

**Honestidade técnica** (mostra maturidade, e é melhor você levantar antes da banca): a chave
JWT está no `appsettings.json` versionado, o que é aceitável em trabalho acadêmico mas em
produção iria para variável de ambiente ou cofre de segredos. E o CORS está aberto, o que
também mudaria em produção.

---

## 12. Trilha de estudo em 5 sessões

Ordem pensada para o entendimento se construir camada sobre camada. Faça com o projeto rodando
e o Swagger aberto ao lado.

### Sessão 1 — Ver funcionando antes de ler código
Suba a API e o front. Entre como professor, olhe as questões, abra uma com imagem. Saia, entre
como aluno, faça o simulado de exemplo inteiro (responda, escreva a redação, finalize, veja o
resultado). Volte como professor e corrija a redação. Volte como aluno e veja a nota aparecer.
**Objetivo:** ter o produto na cabeça antes do código.

### Sessão 2 — O domínio
Leia, nesta ordem: `Models/Usuario.cs`, `Questao.cs`, `Alternativa.cs`, `Prova.cs`,
`TemaRedacao.cs`, `TentativaProva.cs`, `RespostaAluno.cs`. Depois
`Data/BancoQuestoesContext.cs` inteiro.
**Objetivo:** desenhar o diagrama de entidades de memória, num papel.

### Sessão 3 — Autenticação e autorização
`Security/PasswordHasher.cs` → `Security/JwtTokenService.cs` → `Controllers/AuthController.cs`
→ o trecho de JWT em `Program.cs` → `auth.js` no front.
**Teste prático:** faça login pelo Swagger, copie o token, cole em [jwt.io] e veja as claims.
Depois tente `POST /api/questoes` sem token (401) e com token de aluno (403).
**Objetivo:** explicar o ciclo completo do token, do login até o `[Authorize]`.

### Sessão 4 — O fluxo da prova
`Controllers/TentativasController.cs` inteiro, método a método, com
`Responses/TentativaResponses.cs` aberto ao lado. Depois `FazerProva.js` e `ResultadoProva.js`.
**Objetivo:** explicar por que o gabarito não vaza e por que o cronômetro não é burlável.

### Sessão 5 — O entorno
`Controllers/RedacoesController.cs`, `Data/DbSeeder.cs`, `Program.cs` inteiro (a ordem do
pipeline importa), e a seção §9 deste guia.
**Objetivo:** conseguir responder "por que isso está assim?" para qualquer decisão do projeto.

### Exercícios de fixação

Se conseguir fazer estes sem consultar, você domina o sistema:

1. Onde eu mudaria o sistema para uma questão poder ter mais de uma imagem?
2. Que arquivos eu tocaria para adicionar um campo "dificuldade" na questão? (Model → Context →
   Request → Response → Controller → front)
3. Por que `TentativaProva.ExpiraEm` existe, se dá para calcular `IniciadoEm + TempoLimite`?
4. O que acontece se dois alunos fizerem a mesma prova ao mesmo tempo?
5. Por que `Prova` usa `DeleteBehavior.Restrict` e `Questao` usa `Cascade`?

---

## 13. Roteiro de demonstração

Sugestão para 10–12 minutos, na ordem que conta uma história.

| Tempo | O que fazer | O que dizer |
|---|---|---|
| 0–1 min | Slide/fala de abertura | O ponto de partida (banco de questões genérico) e o pivô para plataforma de simulados do ENEM |
| 1–2 min | Mostrar `index.html` logado como professor e depois como aluno | "O menu é montado conforme o papel — mas isso é conveniência; a permissão real está no servidor" |
| 2–4 min | Banco de questões: filtrar por área, abrir uma questão com imagem | Acervo de 539 questões reais, com fonte e ano citados |
| 4–5 min | Montar (ou mostrar montada) uma prova com tempo e tema de redação | Reuso das questões: relação muitos-para-muitos |
| 5–8 min | **Fazer o simulado como aluno** — responder, mostrar o cronômetro, escrever a redação, finalizar, ver o resultado | O ponto alto. Aqui é onde o sistema deixa de ser CRUD |
| 8–9 min | Abrir o DevTools na aba Network durante a prova e mostrar que o gabarito não vem no JSON | Demonstração concreta de segurança |
| 9–10 min | Corrigir a redação como professor e ver a nota no resultado do aluno | Ciclo fechado entre os dois papéis |
| 10–11 min | Swagger + rodar `dotnet test` | Documentação automática e 16 testes passando |
| 11–12 min | Fechamento | Limitações conhecidas e próximos passos (§15) — mostra consciência do próprio trabalho |

**Prepare antes:** os dois navegadores/janelas já logados (um professor, um aluno) para não
perder tempo com login; uma prova curta cadastrada, para dar tempo de finalizar ao vivo.

---

## 14. Perguntas prováveis e como responder

**"Por que ASP.NET Core e não Node/Java/PHP?"**
Tipagem estática ajuda em sistema com regra de negócio (o compilador pega erro de contrato
antes de rodar), o framework já traz autenticação, autorização por papel, injeção de
dependência e documentação automática sem biblioteca externa, e o EF Core mantém o modelo de
dados no próprio código.

**"Por que SQLite e não SQL Server/MySQL?"**
Para o escopo acadêmico, o ganho de qualquer pessoa clonar e rodar sem instalar nada supera a
necessidade de um servidor de banco. Como é EF Core, trocar o provedor é mudar a linha
`UseSqlite(...)` para `UseSqlServer(...)` e ajustar a connection string — o resto do código não
muda. Esse é justamente o valor do ORM.

**"Por que JavaScript puro no front?"**
O front é essencialmente formulários e listas consumindo a API. Um framework traria build,
dependências e complexidade sem benefício proporcional. O foco do trabalho é a API e a regra de
negócio.

**"E se o aluno abrir o DevTools e procurar a resposta?"**
Não encontra. Durante a prova a API responde com `AlternativaProvaResponse`, que só tem `id` e
`descricao`. O campo `Correta` só aparece no `GET /api/tentativas/{id}`, que exige a tentativa
finalizada. Dá para demonstrar ao vivo na aba Network.

**"E se ele mudar o relógio do computador?"**
Só muda a exibição do cronômetro. O prazo real está gravado no banco e é comparado com o
relógio do servidor a cada resposta enviada.

**"E se ele fechar o navegador no meio?"**
Duas coisas: as respostas já dadas estão salvas (cada clique é um `PUT` imediato), e ao voltar
a mesma tentativa é retomada com o prazo original. Se o prazo já tiver estourado, o sistema
fecha a tentativa automaticamente com a nota parcial.

**"Como o sistema calcula a nota?"**
As objetivas, automaticamente no servidor: conta as respostas cuja alternativa selecionada tem
`Correta = true`. A redação, manualmente pelo professor, na escala de 0 a 1000 do ENEM, com
comentário. É a divisão que corresponde ao processo real.

**"Onde estão os testes e o que eles cobrem?"**
16 testes de integração em xUnit que sobem a aplicação real com banco em memória e testam por
HTTP: criação com dados válidos, rejeição de dados inválidos (questão sem alternativa correta,
prova com questão inexistente), busca, atualização e exclusão.

**"Como adicionar mais questões do ENEM?"**
Editando `Data/Seed/questoes_enem.json` e seguindo o mesmo formato — não exige alterar código.
Ou pela tela de cadastro, para questões autorais.

**"O sistema está pronto para produção?"**
Não sem ajustes, e sei quais: a chave JWT precisa sair do arquivo versionado para variável de
ambiente, o CORS precisa ser restrito ao domínio real, e o banco deveria migrar para um SGBD
servidor com migrations em vez de `EnsureCreated()`. A arquitetura suporta as três mudanças
sem reescrita.

---

## 15. Limitações conhecidas

Assumir isso na apresentação é sinal de domínio, não de fraqueza.

1. **Uma questão suporta apenas uma imagem** (`ImagemUrl` é um campo único). Algumas questões
   de 2022–2024 tinham dois gráficos complementares na fonte; só a primeira imagem foi
   importada.
2. **Algumas questões importadas têm tabelas em formato markdown no enunciado** (com `|`
   literais), porque não era viável revisar 535 questões uma a uma. É cosmético — o conteúdo
   está correto e legível.
3. **`Disciplina` das questões de 2022–2024 repete a `Area`**, porque a fonte de dados só
   informa a área ampla do ENEM, não a disciplina específica (Física vs. Química vs. Biologia).
   As 4 questões curadas manualmente têm disciplina específica.
4. **Banco criado com `EnsureCreated()`, sem migrations.** Ótimo para o escopo do trabalho
   (roda sem configuração), mas mudar o modelo exige recriar o banco em vez de aplicar uma
   migração incremental.
5. **Sem paginação na listagem de questões.** Com 539 registros a tela ainda responde bem, mas
   com dezenas de milhares seria necessário paginar.

### Possíveis evoluções

- Turmas: professor agrupa alunos e atribui simulados a uma turma específica
- Relatórios de desempenho por área, para o aluno identificar onde está fraco
- Correção da redação pelas 5 competências do ENEM, em vez de nota única
- Sorteio automático de simulado (ex.: "monte uma prova com 45 questões aleatórias por área")
- Múltiplas imagens por questão

---

## Resumo de uma frase

**CursinhoEnem é uma API REST em ASP.NET Core 8 com front-end em JavaScript puro, onde
professores montam simulados a partir de um acervo de 539 questões reais do ENEM e alunos os
executam com cronômetro e redação — com o servidor controlando prazo, gabarito e correção
automática, e o professor corrigindo a redação.**
