# Relatório de Melhorias — Banco de Questões e Composição de Provas

> Registro de uma rodada de ajustes feita sobre o sistema já em funcionamento, focada em
> usabilidade do professor no dia a dia (cadastrar questões, montar provas) e em aproveitamento
> de tela. Cada seção documenta o problema identificado, o que foi feito e por quê — pensado
> pra servir de base numa apresentação das melhorias.

---

## Resumo executivo

| Área | O que mudou |
|---|---|
| Cadastro de questões | Removidos campos que não faziam mais sentido pro produto (Disciplina, Assuntos); nomes de área do ENEM simplificados na exibição |
| Banco de questões | Scroll infinito trocado por paginação (20 por página); campo Disciplina removido do sistema inteiro |
| Composição de prova | Filtro por disciplina/assunto trocado por busca por palavra no enunciado + filtro por área; confirmação visual ao adicionar questão; campo Turma passou a ser salvo de verdade |
| Layout | Containers mais largos e listas de questões em grade responsiva (várias colunas) — bem menos espaço de tela desperdiçado em monitores largos |

---

## 1. Cadastro de questões: remoção de campos que não se aplicavam mais

**Problema:** o formulário de cadastro (`Questao.html`) ainda tinha os campos "Disciplina" e
"Assuntos", herdados da versão original do sistema (um banco de questões genérico, de qualquer
disciplina). Depois do pivô pra uma plataforma de simulados do ENEM — onde a classificação
relevante é a **Área de Conhecimento** (Linguagens, Ciências Humanas, Ciências da Natureza,
Matemática) —, esses dois campos ficaram sem função clara: não estava óbvio o que preencher ali.

**O que foi feito:** os campos Disciplina e Assuntos foram retirados do formulário de
cadastro/edição de questão. Os nomes das áreas do ENEM, que apareciam por extenso ("Linguagens,
Códigos e suas Tecnologias"), passaram a ser exibidos de forma resumida ("Linguagens, Códigos")
em todas as telas — o valor completo continua sendo o que fica salvo no banco, então nenhuma
questão já cadastrada precisou ser alterada.

## 2. Remoção do campo Disciplina de todo o sistema

**Problema:** ao investigar por que "Disciplina" parecia redundante, foi encontrado que, das 539
questões do acervo, **535 tinham o campo Disciplina idêntico ao campo Área** (ex.: os dois campos
vinham preenchidos exatamente com "Matemática e suas Tecnologias") — a fonte de dados usada pra
importar as questões de 2022 a 2024 não trazia a disciplina específica de cada questão, só a área
ampla do ENEM. Só as 4 questões cadastradas manualmente tinham uma disciplina de fato diferente
da área (ex.: "História", "Biologia").

**Decisão:** em vez de manter um campo majoritariamente redundante — ou tentar adivinhar a
disciplina certa de 535 questões uma a uma, o que exigiria revisão manual —, o campo Disciplina
foi **removido do sistema inteiro**: não só da tela, mas do modelo de dados, das requisições e
respostas da API, dos controllers, do populador do banco (seeder) e dos testes automatizados.
Toda tela que ainda exibia "Disciplina" (lista de provas, seleção de prova) passou a mostrar a
nova informação de Turma no lugar (ver item 6).

## 3. Paginação no banco de questões e na composição de prova

**Problema:** com 539 questões no acervo, tanto a tela "Banco de Questões" quanto a lista de
questões disponíveis na hora de montar uma prova carregavam tudo de uma vez, obrigando a rolar a
página indefinidamente até achar uma questão.

**O que foi feito:** a API de listagem de questões (`GET /api/questoes`) passou a aceitar
parâmetros de página (`pagina`, `tamanhoPagina`, padrão 20), devolvendo o total de páginas junto
com os resultados. As duas telas ganharam botões de navegação (Anterior / números de página /
Próxima) no rodapé da lista, no lugar do scroll infinito.

## 4. Busca por palavra e filtro por área na composição de prova

**Problema:** o único jeito de filtrar questões na hora de montar uma prova era por disciplina ou
assunto — filtros que, além de dependerem de campos pouco preenchidos (ver item 2), não ajudavam
a achar uma questão pelo *conteúdo* dela (ex.: "quero todas as questões que falem sobre
racismo").

**O que foi feito:** os filtros de disciplina/assunto foram substituídos por uma busca por
palavra no enunciado da questão (ex.: `busca=racismo` retorna todas as questões cujo texto
contém essa palavra) e por um filtro por área de conhecimento do ENEM — os dois podem ser usados
juntos ou separados.

## 5. Confirmação visual ao adicionar uma questão à prova

**Problema:** ao clicar "Adicionar" numa questão durante a composição de uma prova, não havia
nenhum retorno visual imediato confirmando que a questão realmente entrou na lista — era preciso
rolar até a seção "Questões Selecionadas" pra conferir.

**O que foi feito:** ao adicionar uma questão, aparece uma mensagem de confirmação temporária na
tela ("Questão adicionada à prova.") e o próprio botão daquela questão muda pra
"✓ Adicionada" e fica desabilitado — inclusive se o professor filtrar de novo depois, questões já
adicionadas continuam marcadas assim.

## 6. Campo Turma passou a ser salvo de verdade

**Problema:** o formulário de criação de prova sempre teve um campo "Turma/Semestre", mas ao
revisar o código foi encontrado que esse campo **nunca era enviado pra API** — era um campo
"morto" na tela, que o professor preenchia sem nenhum efeito.

**O que foi feito:** o campo foi simplificado pra "Número da Turma" e, dessa vez, corretamente
ligado ao banco de dados (`Prova.Turma`) — agora aparece de verdade na lista de provas do
professor e na tela de seleção de prova do aluno.

## 7. Aproveitamento de espaço em tela (responsividade)

**Problema:** em monitores largos, o conteúdo do sistema ficava confinado a uma coluna fixa de
800px, deixando uma faixa grande de tela vazia dos dois lados — e as listas de questões
empilhavam um card embaixo do outro numa coluna só, mesmo havendo espaço de sobra pra mostrar
várias lado a lado.

**O que foi feito:**
- As listas de questões (banco de questões, questões disponíveis/selecionadas na composição de
  prova, lista de provas, temas de redação, redações entregues, histórico do aluno) passaram a
  usar uma grade responsiva: quantas colunas couberem no espaço disponível aparecem lado a lado
  (até 4 numa tela larga), em vez de uma coluna única. Em telas de celular continua em coluna
  única, sem quebrar.
- O espaço reservado ao conteúdo cresceu de 800px pra até 1400px nas telas que se beneficiam
  disso (listagens).
- Telas que são só formulário (login, criar conta, criar questão, criar tema de redação, corrigir
  redação, fazer prova, ver resultado) foram mantidas numa largura mais confortável de leitura
  (~800px) — um campo de texto de uma linha só esticado até 1400px ficaria com cara de formulário
  quebrado, então essas telas não usam o espaço extra.
- Na composição de prova, que mistura formulário (título, turma, filtros) com a grade de
  questões, só a parte de formulário ficou numa faixa mais estreita (760px) — a grade de questões
  usa a largura total disponível.

## 8. Problemas técnicos enfrentados no caminho (fora do código em si)

Dois problemas de ambiente, não de código, valem registrar porque tomaram tempo de diagnóstico:

- **Build travado por processo em segundo plano:** depois de testar a API rodando localmente, o
  processo continuou de pé em segundo plano e ficou segurando (bloqueando) o arquivo executável —
  qualquer tentativa de recompilar o projeto falhava com erro de arquivo em uso (`MSB3027`).
  Resolvido encerrando o processo travado.
- **Cache do navegador mascarando as mudanças:** por duas vezes, o navegador continuou usando uma
  versão antiga (em cache) dos arquivos `.js`, mesmo já com o código corrigido e o servidor já
  servindo a versão nova — isso se manifestou como "Erro ao carregar as questões" e "formatArea
  is not defined", dois sintomas que pareciam bugs de código mas eram só o navegador não buscando
  a versão atual dos arquivos. Confirmado comparando o que o servidor realmente devolvia com o
  que o navegador mostrava; resolvido com atualização forçada da página (Ctrl+Shift+R).

---

Esse conjunto de mudanças está registrado no histórico de commits do repositório, um commit por
tema tratado nas seções acima.
