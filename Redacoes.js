// ===============================
// Script da Página de Listagem de Redações (professor)
// ===============================

const container = document.getElementById("listaRedacoes");

authFetch(`${API_BASE}/redacoes`)
    .then(response => response.json())
    .then(redacoes => {
        container.innerHTML = "";

        if (redacoes.length === 0) {
            container.innerHTML = "<p>Nenhuma redação para corrigir ainda.</p>";
            return;
        }

        redacoes.forEach(redacao => {
            const card = document.createElement("div");
            card.className = "question-card";

            const data = new Date(redacao.finalizadoEm).toLocaleString("pt-BR");
            const statusHtml = redacao.corrigida
                ? `<span class="status-correta">Corrigida — ${redacao.notaRedacao} / 1000</span>`
                : `<span class="badge-tipo">Pendente</span>`;

            card.innerHTML = `
                <h3>${redacao.alunoNome}</h3>
                <p><strong>Prova:</strong> ${redacao.provaTitulo}</p>
                <p><strong>Tema:</strong> ${redacao.temaRedacaoTitulo}</p>
                <p><strong>Finalizada em:</strong> ${data}</p>
                <p>${statusHtml}</p>
                <div class="acoes">
                    <a class="btn" href="CorrigirRedacao.html?id=${redacao.idTentativa}">${redacao.corrigida ? "Ver/Editar Correção" : "Corrigir"}</a>
                </div>
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar as redações.</p>";
    });
