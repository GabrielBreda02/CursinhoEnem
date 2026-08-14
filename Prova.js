const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const tituloInput = document.getElementById("titulo");
const disciplinaInput = document.getElementById("disciplina");
const filtroDisciplinaInput = document.getElementById("filtroDisciplina");
const filtroAssuntoInput = document.getElementById("filtroAssunto");
const listaBanco = document.getElementById("tabelaQuestoes");
const listaSelecionadas = document.getElementById("tabelaSelecionadas");

let questoesSelecionadas = [];

if (id) {
    fetch(`${API_BASE}/Provas/${id}`)
        .then(res => res.json())
        .then(prova => {
            tituloInput.value = prova.titulo;
            disciplinaInput.value = prova.disciplina;
            questoesSelecionadas = prova.questoes.map(q => q.idQuestao);
            carregarQuestoesSelecionadas();
        });
}

function buscarQuestoes() {
    const disciplina = filtroDisciplinaInput.value;
    const assunto = filtroAssuntoInput.value;
    fetch(`${API_BASE}/questoes?disciplina=${disciplina}&assunto=${assunto}`, {
        method: "GET"
    })
        .then(res => res.json())
        .then(questoes => {
            listaBanco.innerHTML = "";

            if (questoes.length === 0) {
                listaBanco.innerHTML = "<p>Nenhuma questão encontrada.</p>";
                return;
            }

            questoes.forEach(questao => {
                const div = document.createElement("div");
                div.className = "question-card";

                div.innerHTML = `
                    <h4>${questao.titulo}</h4>
                    <p>${questao.assuntos.join(", ")}</p>
                    <button onclick="adicionarQuestao(${questao.idQuestao})" class="btn">Adicionar</button>
                `;

                listaBanco.appendChild(div);
            });
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
                    <h4>${questao.titulo}</h4>
                    <p>${questao.assuntos.join(", ")}</p>
                    <button onclick="removerQuestao(${questao.idQuestao})" class="btn btn-danger">Remover</button>
                `;

                listaSelecionadas.appendChild(div);
            });
    });
}

function salvarProva() {
    const dados = {
        titulo: tituloInput.value.trim(),
        disciplina: disciplinaInput.value.trim(),
        QuestoesIds: questoesSelecionadas
    };

    if (!dados.titulo || !dados.disciplina) {
        alert("Preencha o título e a disciplina.");
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
