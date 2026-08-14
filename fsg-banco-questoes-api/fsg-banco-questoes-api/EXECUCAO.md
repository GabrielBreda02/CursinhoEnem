# 🚀 Como Executar a API

## Pré-requisitos
- **.NET 8 SDK** instalado no sistema

## 📋 Passos para Executar

### 1. Abrir Terminal/Prompt
Navegue até a pasta do projeto:
```bash
cd banco-questoes-api
```

### 2. Restaurar Dependências (Primeira Execução)
```bash
dotnet restore
```

### 3. Executar a API
```bash
dotnet run --project BancoQuestoes.Api.csproj
```

**OU** simplesmente:
```bash
dotnet run
```

### 4. Acessar o Swagger
🎉 **O navegador abrirá automaticamente** na URL da documentação Swagger!

**URLs disponíveis:**
- **Swagger UI:** `http://localhost:5000` ou `https://localhost:5001` (raiz da aplicação)
- **API Endpoints:** `http://localhost:5000/api/` ou `https://localhost:5001/api/`
- **Swagger JSON:** `http://localhost:5000/swagger/v1/swagger.json`

## 🔧 Configurações do Swagger

✅ **Swagger configurado para:**
- Abrir automaticamente ao iniciar a aplicação
- Estar disponível na **raiz da aplicação** (`/`)
- Funcionar em **qualquer ambiente** (Development, Production, etc.)
- Mostrar documentação detalhada de todos os endpoints
- Exibir exemplos de request/response
- Validar requests em tempo real
- Mostrar tempo de duração das requisições

## 📱 Testando os Endpoints

### Criar uma Questão (POST /api/questoes)
```json
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

### Criar uma Prova (POST /api/provas)
```json
{
  "titulo": "Prova de Geografia Básica",
  "disciplina": "Geografia",
  "questoesIds": [1, 2, 3]
}
```

### Filtrar Questões
- `GET /api/questoes?disciplina=Geografia`
- `GET /api/questoes?assunto=Brasil`
- `GET /api/questoes?disciplina=Geografia&assunto=Capitais`

## 💾 Banco de Dados
- O banco SQLite (`bancoquestoes.db`) é criado automaticamente na primeira execução
- Localizado na raiz do projeto
- Não requer configuração adicional

## ⚠️ Solução de Problemas

**Se o navegador não abrir automaticamente:**
1. Abra manualmente: `http://localhost:5000`
2. Verifique se a porta não está ocupada
3. Tente usar HTTPS: `https://localhost:5001`

**Se houver erro de porta ocupada:**
```bash
dotnet run --urls "http://localhost:5002"
```

## 🛑 Parar a Aplicação
No terminal onde a aplicação está rodando:
- **Windows:** `Ctrl + C`
- **Linux/Mac:** `Ctrl + C` 