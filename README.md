# CursinhoEnem

Plataforma web de simulados para quem está estudando para o ENEM — cursinhos ou alunos por
conta própria. Professores montam o banco de questões e as provas; alunos fazem os simulados
com cronômetro e escrevem a redação, e depois acompanham nota e histórico.

Back-end em ASP.NET Core 8 (API REST) e front-end em HTML, CSS e JavaScript puro, sem
frameworks.

## Funcionalidades

- Autenticação com dois perfis: **Professor** (cadastra conteúdo) e **Aluno** (faz simulados)
- Banco de questões categorizado pelas 4 áreas do ENEM, com upload de imagem no enunciado
  (gráficos, tirinhas, mapas)
- Composição de provas com tempo limite configurável e tema de redação
- Simulado do aluno com cronômetro regressivo controlado pelo servidor, correção automática
  das questões objetivas e área para escrever a redação
- Resultado detalhado (acerto/erro por questão) e histórico de simulados feitos
- Acervo inicial com questões reais do ENEM (citadas com ano e fonte) e formato de importação
  pronto para receber mais conteúdo — veja
  `fsg-banco-questoes-api/fsg-banco-questoes-api/Data/Seed/questoes_enem.json`

## Como executar

**API** (porta 5000):
```
cd fsg-banco-questoes-api/fsg-banco-questoes-api
dotnet run --project BancoQuestoes.Api.csproj
```
O banco SQLite é criado automaticamente na primeira execução, já com um usuário de teste de
cada perfil (`professor@teste.com` / `aluno@teste.com`, senha `senha123`), o acervo curado do
ENEM e uma prova de exemplo prontos para usar.

**Front-end**: sirva a pasta raiz como arquivos estáticos, por exemplo:
```
python -m http.server 8080
```
e acesse `http://localhost:8080/index.html`.

## Segurança

- Senhas com hash PBKDF2 (salt aleatório por usuário)
- Autenticação por JWT, com o papel do usuário (Professor/Aluno) como claim de role
- Endpoints de escrita exigem login e o papel correto; leitura de questões/provas é pública
- Durante o simulado, a API nunca revela qual alternativa é a correta — só depois de finalizada
- O prazo do simulado é controlado pelo servidor, não pelo relógio do navegador do aluno
