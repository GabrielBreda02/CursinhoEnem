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

            const imagemHtml = questao.imagemUrl
                ? `<img src="${API_BASE.replace(/\/api$/, "")}${questao.imagemUrl}" style="max-width:100%;border-radius:8px;margin:8px 0;">`
                : "";
            const fonteHtml = questao.fonte
                ? `<p><strong>Fonte:</strong> ${questao.fonte}${questao.ano ? " (" + questao.ano + ")" : ""}</p>`
                : "";

            card.innerHTML = `
                <h3>${questao.titulo}</h3>
                ${imagemHtml}
                <p><strong>Área:</strong> ${questao.area || "-"}</p>
                <p><strong>Disciplina:</strong> ${questao.disciplina}</p>
                <p><strong>Assuntos:</strong> ${questao.assuntos.join(", ")}</p>
                ${fonteHtml}
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

