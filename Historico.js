// ===============================
// Script da Página de Histórico de Tentativas (aluno)
// ===============================

const container = document.getElementById("listaHistorico");

authFetch(`${API_BASE}/tentativas/minhas`)
    .then(response => response.json())
    .then(tentativas => {
        container.innerHTML = "";

        if (tentativas.length === 0) {
            container.innerHTML = "<p>Você ainda não fez nenhuma prova.</p>";
            return;
        }

        tentativas.forEach(tentativa => {
            const card = document.createElement("div");
            card.className = "question-card";

            const data = new Date(tentativa.iniciadoEm).toLocaleString("pt-BR");
            const finalizada = !!tentativa.finalizadoEm;

            const statusHtml = finalizada
                ? `<p><strong>Nota:</strong> ${tentativa.notaObjetivas} de ${tentativa.totalQuestoes} questões</p>
                   <div class="acoes"><a class="btn" href="ResultadoProva.html?id=${tentativa.idTentativa}">Ver Resultado</a></div>`
                : `<p><strong>Status:</strong> Em andamento (não finalizada)</p>`;

            card.innerHTML = `
                <h3>${tentativa.provaTitulo}</h3>
                <p><strong>Data:</strong> ${data}</p>
                ${statusHtml}
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar o histórico.</p>";
    });
