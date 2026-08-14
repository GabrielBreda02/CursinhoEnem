# Banco de Questões API

Uma Web API desenvolvida em .NET 8 usando Minimal APIs para gerenciamento de banco de questões e composição de provas.

## Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **Minimal APIs** - Para criação dos endpoints
- **Entity Framework Core** - ORM para acesso a dados
- **SQLite** - Banco de dados
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Framework de testes
- **Microsoft.AspNetCore.Mvc.Testing** - Testes de integração

## Estrutura do Projeto

```
├── Models/                 # Entidades do domínio
│   ├── Questao.cs         # Entidade Questão
│   ├── Alternativa.cs     # Entidade Alternativa
│   └── Prova.cs           # Entidade Prova
├── DTOs/                  # Data Transfer Objects
│   ├── QuestaoListDto.cs
│   ├── QuestaoDetailDto.cs
│   ├── ProvaListDto.cs
│   ├── ProvaDetailDto.cs
│   ├── CreateQuestaoDto.cs
│   └── CreateProvaDto.cs
├── Data/                  # Contexto do banco de dados
│   └── BancoQuestoesContext.cs
├── Tests/                 # Testes de integração
│   ├── IntegrationTestBase.cs
│   ├── QuestoesIntegrationTests.cs
│   └── ProvasIntegrationTests.cs
└── Program.cs             # Configuração da aplicação e endpoints
```

## Funcionalidades

### Entidade Questão
- **Id, Título, Disciplina, Assuntos, Alternativas**
- CRUD completo
- Filtros por disciplina e assunto
- Validação de pelo menos uma alternativa correta

### Entidade Prova
- **Id, Título, Disciplina, Lista de Questões**
- CRUD completo
- Criação baseada em IDs de questões existentes
- Contagem automática de questões

## Endpoints da API

### Questões

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/questoes` | Lista todas as questões |
| GET | `/api/questoes?disciplina=X&assunto=Y` | Filtra questões por disciplina e/ou assunto |
| GET | `/api/questoes/{id}` | Obtém questão específica com alternativas |
| POST | `/api/questoes` | Cria nova questão |
| PUT | `/api/questoes/{id}` | Atualiza questão existente |
| DELETE | `/api/questoes/{id}` | Remove questão |

### Provas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/provas` | Lista todas as provas |
| GET | `/api/provas/{id}` | Obtém prova específica com questões |
| POST | `/api/provas` | Cria nova prova |
| PUT | `/api/provas/{id}` | Atualiza prova existente |
| DELETE | `/api/provas/{id}` | Remove prova |

## Como Executar

### Pré-requisitos
- .NET 8 SDK

### Execução da API

1. **Clone o repositório**
   ```bash
   git clone <repo-url>
   cd banco-questoes-api
   ```

2. **Restaure as dependências**
   ```bash
   dotnet restore
   ```

3. **Execute a aplicação**
   ```bash
   dotnet run --project BancoQuestoes.Api.csproj
   ```

4. **Acesse a documentação**
   - Swagger UI: `https://localhost:5001/swagger`
   - API: `https://localhost:5001/api/`

### Execução dos Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Banco de Dados

O banco SQLite é criado automaticamente na primeira execução. O arquivo `bancoquestoes.db` será gerado na raiz do projeto.

### Estrutura das Tabelas

- **Questoes**: Id, Titulo, Disciplina, AssuntosJson
- **Alternativas**: Id, Descricao, Correta, QuestaoId
- **Provas**: Id, Titulo, Disciplina
- **ProvaQuestao**: Tabela de relacionamento N:N entre Provas e Questões

## Exemplos de Uso

### Criar uma Questão

```json
POST /api/questoes
{
  "titulo": "Qual é a capital do Brasil?",
  "disciplina": "Geografia",
  "assuntos": ["Capitais", "Brasil"],
  "alternativas": [
    {
      "descricao": "São Paulo",
      "correta": false
    },
    {
      "descricao": "Brasília",
      "correta": true
    },
    {
      "descricao": "Rio de Janeiro",
      "correta": false
    }
  ]
}
```

### Criar uma Prova

```json
POST /api/provas
{
  "titulo": "Prova de Geografia",
  "disciplina": "Geografia",
  "questoesIds": [1, 2, 3]
}
```

## Validações

### Questões
- Título é obrigatório (máx. 500 caracteres)
- Disciplina é obrigatória (máx. 100 caracteres)
- Mínimo de 2 alternativas
- Pelo menos uma alternativa correta

### Provas
- Título é obrigatório (máx. 200 caracteres)
- Disciplina é obrigatória (máx. 100 caracteres)
- Mínimo de 1 questão
- Todas as questões devem existir

## Testes

O projeto inclui testes de integração abrangentes que cobrem:

- **Testes de Questões**: CRUD completo, validações, filtros
- **Testes de Provas**: CRUD completo, validações, relacionamentos
- **Cenários de Erro**: Dados inválidos, recursos não encontrados
- **Banco em Memória**: Isolamento entre testes

### Cobertura de Testes

- ✅ GET endpoints com dados vazios
- ✅ POST com dados válidos e inválidos
- ✅ PUT para atualização
- ✅ DELETE para remoção
- ✅ Filtros e consultas específicas
- ✅ Validações de negócio
- ✅ Relacionamentos entre entidades 