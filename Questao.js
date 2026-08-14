// ===============================
// Script da Página de Cadastro/Edição de Questões
// ===============================

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const tituloInput = document.getElementById("titulo");
const disciplinaInput = document.getElementById("disciplina");
const assuntosInput = document.getElementById("assuntos");


const alternativasContainer = document.getElementById("alternativa");

if (id) {
    fetch(`${API_BASE}/questoes/${id}`)
        .then(res => res.json())
        .then(questao => {
            tituloInput.value = questao.titulo;
            disciplinaInput.value = questao.disciplina;
            assuntosInput.value = questao.assuntos.join(", ");

            alternativasContainer.innerHTML = "";
            questao.alternativas.forEach((alt, index) => {
                adicionarAlternativa(alt.descricao, alt.correta);
            });
        });
} else {
    // Adiciona 4 alternativas vazias por padrão no modo "Create"
    for (let i = 0; i < 5; i++) {
        adicionarAlternativa();
    }
}


function adicionarAlternativa(descricao = "", correta = false) {
    // Cria a div que vai conter a alternativa
    const divAlternativa = document.createElement("div");
    divAlternativa.classList.add("alternativa");

    // Cria o input tipo radio
    const inputRadio = document.createElement("input");
    inputRadio.type = "radio";
    inputRadio.name = "correta";
    inputRadio.value = "0"; // Pode ser ajustado se precisar identificar por índice
    if (correta) {
        inputRadio.checked = true;
    }

    // Cria o input de texto com a descrição da alternativa
    const inputTexto = document.createElement("input");
    inputTexto.type = "text";
    inputTexto.classList.add("opcao");
    inputTexto.placeholder = "Opção";
    inputTexto.value = descricao;

    // Adiciona os inputs dentro da div
    divAlternativa.appendChild(inputRadio);
    divAlternativa.appendChild(inputTexto);

    // Adiciona a div ao container de alternativas
    alternativasContainer.appendChild(divAlternativa);
}


function obterDadosQuestao() {
    const assuntos = assuntosInput.value.split(",").map(a => a.trim()).filter(a => a);
    const alternativas = Array.from(document.querySelectorAll(".alternativa")).map(alt => ({
        descricao: alt.querySelector("input[type=text]").value,
        correta: alt.querySelector("input[type=radio]").checked
    }));

    return {
        titulo: tituloInput.value,
        disciplina: disciplinaInput.value,
        assuntos,
        alternativas
    };
}

function salvarQuestao() {
    const dados = obterDadosQuestao();
    const metodo = id ? "PUT" : "POST";
    const url = id
        ? `${API_BASE}/questoes/${id}`
        : `${API_BASE}/questoes`;

    authFetch(url, {
        method: metodo,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dados)
    })
        .then(response => {
            console.log(response)
            if (!response.ok) {
                throw new Error("Erro na requisição: " + response.statusText);
            }
            return response.json();
        })
        .then(() => window.location.href = "questoes.html")
        .catch(erro => console.error("Erro ao salvar questão:", erro));
}

function excluirQuestao() {
    if (!id) return;

    if (confirm("Deseja excluir essa questão?")) {
        authFetch(`${API_BASE}/questoes/${id}`, {
            method: "DELETE"
        }).then(() => window.location.href = "questoes.html");
    }
}
