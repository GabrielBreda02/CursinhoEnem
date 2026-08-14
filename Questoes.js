// ===============================
// Script da Página Banco de Questões
// ===============================

const container = document.getElementById("listaQuestoes");

fetch(`${API_BASE}/questoes`, {
    method: "GET"
})
    .then(response => response.json())
    .then(questoes => {
        container.innerHTML = "";

        if (questoes.length === 0) {
            container.innerHTML = "<p>Nenhuma questão cadastrada.</p>";
            return;
        }

        questoes.forEach(questao => {
            const card = document.createElement("div");
            card.className = "question-card";

            card.innerHTML = `
                <h3>${questao.titulo}</h3>
                <p><strong>Disciplina:</strong> ${questao.disciplina}</p>
                <p><strong>Assuntos:</strong> ${questao.assuntos.join(", ")}</p>
                <div class="acoes">
                    <a class="btn" href="questao.html?id=${questao.idQuestao}">Editar</a>
                     <button type="button" onclick="excluirQuestao(${questao.idQuestao})" class="btn btn-danger"> Excluir</button>
                </div>
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar as questões.</p>";
    });
function excluirQuestao(idQuestao) {
    if (!idQuestao) return;

    if (confirm("Deseja excluir essa questão?")) {
        authFetch(`${API_BASE}/questoes/${idQuestao}`, {
            method: "DELETE"
        }).then(() => window.location.href = "Questoes.html");
    }
}

