const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const tituloInput = document.getElementById("titulo");
const turmaInput = document.getElementById("turma");
const tempoLimiteInput = document.getElementById("tempoLimite");
const temaRedacaoSelect = document.getElementById("temaRedacao");
const filtroBuscaInput = document.getElementById("filtroBusca");
const listaBanco = document.getElementById("tabelaQuestoes");
const listaSelecionadas = document.getElementById("tabelaSelecionadas");
const paginacaoQuestoesContainer = document.getElementById("paginacaoQuestoes");

const TAMANHO_PAGINA = 20;

let questoesSelecionadas = [];
let paginaAtualQuestoes = 1;

fetch(`${API_BASE}/temas-redacao`)
    .then(res => res.json())
    .then(temas => {
        temas.forEach(tema => {
            const option = document.createElement("option");
            option.value = tema.idTemaRedacao;
            option.textContent = tema.titulo;
            temaRedacaoSelect.appendChild(option);
        });

        if (id) {
            fetch(`${API_BASE}/Provas/${id}`)
                .then(res => res.json())
                .then(prova => {
                    tituloInput.value = prova.titulo;
                    turmaInput.value = prova.turma || "";
                    tempoLimiteInput.value = prova.tempoLimiteMinutos;
                    temaRedacaoSelect.value = prova.temaRedacaoId || "";
                    questoesSelecionadas = prova.questoes.map(q => q.idQuestao);
                    carregarQuestoesSelecionadas();
                });
        }
    });

buscarQuestoes();

function buscarQuestoes(pagina = 1) {
    paginaAtualQuestoes = pagina;

    const query = new URLSearchParams({ pagina, tamanhoPagina: TAMANHO_PAGINA });
    const busca = filtroBuscaInput.value.trim();
    if (busca) {
        query.set("busca", busca);
    }

    fetch(`${API_BASE}/questoes?${query.toString()}`, {
        method: "GET"
    })
        .then(res => res.json())
        .then(({ itens: questoes, paginaAtual, totalPaginas }) => {
            listaBanco.innerHTML = "";

            if (questoes.length === 0) {
                listaBanco.innerHTML = "<p>Nenhuma questão encontrada.</p>";
            }

            questoes.forEach(questao => {
                const div = document.createElement("div");
                div.className = "question-card";

                div.innerHTML = `
                    ${questao.area ? `<span class="badge-area">${formatArea(questao.area)}</span>` : ""}
                    <h4>${questao.titulo}</h4>
                    <button onclick="adicionarQuestao(${questao.idQuestao})" class="btn">Adicionar</button>
                `;

                listaBanco.appendChild(div);
            });

            paginacaoQuestoesContainer.innerHTML = "";
            paginacaoQuestoesContainer.appendChild(criarControlesPaginacao(paginaAtual, totalPaginas, buscarQuestoes));
        });
}

function adicionarQuestao(idQuestao) {
    if (!questoesSelecionadas.includes(idQuestao)) {
        questoesSelecionadas.push(idQuestao);
        carregarQuestoesSelecionadas();
    }
}

function removerQuestao(idQuestao) {
    questoesSelecionadas = questoesSelecionadas.filter(id => id !== idQuestao);
    carregarQuestoesSelecionadas();
}

function carregarQuestoesSelecionadas() {
    listaSelecionadas.innerHTML = "";

    questoesSelecionadas.forEach(idQuestao => {
        fetch(`${API_BASE}/questoes/${idQuestao}`)
            .then(res => res.json())
            .then(questao => {
                const div = document.createElement("div");
                div.className = "question-card";

                div.innerHTML = `
                    ${questao.area ? `<span class="badge-area">${formatArea(questao.area)}</span>` : ""}
                    <h4>${questao.titulo}</h4>
                    <button onclick="removerQuestao(${questao.idQuestao})" class="btn btn-danger">Remover</button>
                `;

                listaSelecionadas.appendChild(div);
            });
    });
}

function salvarProva() {
    const dados = {
        titulo: tituloInput.value.trim(),
        turma: turmaInput.value.trim() || null,
        tempoLimiteMinutos: Number(tempoLimiteInput.value) || 180,
        temaRedacaoId: temaRedacaoSelect.value ? Number(temaRedacaoSelect.value) : null,
        QuestoesIds: questoesSelecionadas
    };

    if (!dados.titulo) {
        alert("Preencha o título da prova.");
        return;
    }

    const metodo = id ? "PUT" : "POST";
    const url = id
        ? `${API_BASE}/Provas/${id}`
        : `${API_BASE}/Provas`;

    authFetch(url, {
        method: metodo,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dados)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Erro na requisição');
            }
            alert("Prova salva com sucesso!");
            window.location.href = "provas.html";
        })
        .catch(erro => {
            console.error("Erro ao salvar prova:", erro);
            alert("Erro ao salvar prova.");
        });
}


function excluirProva() {
    if (!id) return;

    if (confirm("Deseja excluir essa prova?")) {
        authFetch(`${API_BASE}/Provas/${id}`, {
            method: "DELETE"
        }).then(() => window.location.href = "provas.html");
    }
}
