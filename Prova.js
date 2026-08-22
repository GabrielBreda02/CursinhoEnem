const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const tituloInput = document.getElementById("titulo");
const turmaInput = document.getElementById("turma");
const tempoLimiteInput = document.getElementById("tempoLimite");
const temaRedacaoSelect = document.getElementById("temaRedacao");
const qtdSorteioInput = document.getElementById("qtdSorteio");
const filtroBuscaInput = document.getElementById("filtroBusca");
const filtroAreaInput = document.getElementById("filtroArea");
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
    });

authFetch(`${API_BASE}/turmas`)
    .then(res => res.json())
    .then(turmas => {
        turmas.forEach(turma => {
            const option = document.createElement("option");
            option.value = turma.idTurma;
            option.textContent = turma.nome;
            turmaInput.appendChild(option);
        });

        if (id) {
            fetch(`${API_BASE}/Provas/${id}`)
                .then(res => res.json())
                .then(prova => {
                    tituloInput.value = prova.titulo;
                    turmaInput.value = prova.turmaId || "";
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
    const area = filtroAreaInput.value;
    if (area) {
        query.set("area", area);
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
                const jaAdicionada = questoesSelecionadas.includes(questao.idQuestao);

                div.innerHTML = `
                    ${questao.area ? `<span class="badge-area">${formatArea(questao.area)}</span>` : ""}
                    <h4>${questao.titulo}</h4>
                    <button type="button" class="btn" onclick="adicionarQuestao(${questao.idQuestao}, this)" ${jaAdicionada ? "disabled" : ""}>${jaAdicionada ? "✓ Adicionada" : "Adicionar"}</button>
                `;

                listaBanco.appendChild(div);
            });

            paginacaoQuestoesContainer.innerHTML = "";
            paginacaoQuestoesContainer.appendChild(criarControlesPaginacao(paginaAtual, totalPaginas, buscarQuestoes));
        });
}

function adicionarQuestao(idQuestao, botao) {
    if (questoesSelecionadas.includes(idQuestao)) {
        return;
    }

    questoesSelecionadas.push(idQuestao);
    carregarQuestoesSelecionadas();

    if (botao) {
        botao.textContent = "✓ Adicionada";
        botao.disabled = true;
    }

    mostrarToast("Questão adicionada à prova.");
}

function sortearQuestoes() {
    const porArea = Number(qtdSorteioInput.value);

    if (!porArea || porArea < 1) {
        alert("Informe quantas questões sortear por área.");
        return;
    }

    authFetch(`${API_BASE}/questoes/sortear?porArea=${porArea}`)
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível sortear as questões.");
            }
            return dados;
        })
        .then(idsSorteados => {
            const novas = idsSorteados.filter(idQuestao => !questoesSelecionadas.includes(idQuestao));
            questoesSelecionadas.push(...novas);
            carregarQuestoesSelecionadas();
            buscarQuestoes(paginaAtualQuestoes);
            mostrarToast(`${novas.length} questões sorteadas e adicionadas à prova.`);
        })
        .catch(erro => {
            console.error(erro);
            alert(erro.message);
        });
}

function mostrarToast(mensagem) {
    const existente = document.querySelector(".toast");
    if (existente) {
        existente.remove();
    }

    const toast = document.createElement("div");
    toast.className = "toast";
    toast.textContent = mensagem;
    document.body.appendChild(toast);

    setTimeout(() => toast.remove(), 2500);
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
        turmaId: turmaInput.value ? Number(turmaInput.value) : null,
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
