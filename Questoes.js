// ===============================
// Script da Página Banco de Questões
// ===============================

const container = document.getElementById("listaQuestoes");
const paginacaoContainer = document.getElementById("paginacao");
const TAMANHO_PAGINA = 20;

let paginaAtual = 1;

function carregarQuestoes(pagina = 1) {
    paginaAtual = pagina;

    fetch(`${API_BASE}/questoes?pagina=${pagina}&tamanhoPagina=${TAMANHO_PAGINA}`, {
        method: "GET"
    })
        .then(response => response.json())
        .then(({ itens: questoes, paginaAtual: pagina, totalPaginas }) => {
            container.innerHTML = "";

            if (questoes.length === 0) {
                container.innerHTML = "<p>Nenhuma questão cadastrada.</p>";
                paginacaoContainer.innerHTML = "";
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
                const assuntosHtml = questao.assuntos.length
                    ? `<p><strong>Assuntos:</strong> ${questao.assuntos.join(", ")}</p>`
                    : "";

                card.innerHTML = `
                    ${questao.area ? `<span class="badge-area">${formatArea(questao.area)}</span>` : ""}
                    <h3>${questao.titulo}</h3>
                    ${imagemHtml}
                    ${assuntosHtml}
                    ${fonteHtml}
                    <div class="acoes">
                        <a class="btn" href="questao.html?id=${questao.idQuestao}">Editar</a>
                        <button type="button" onclick="excluirQuestao(${questao.idQuestao})" class="btn btn-danger">Excluir</button>
                    </div>
                `;

                container.appendChild(card);
            });

            paginacaoContainer.innerHTML = "";
            paginacaoContainer.appendChild(criarControlesPaginacao(pagina, totalPaginas, carregarQuestoes));
        })
        .catch(() => {
            container.innerHTML = "<p>Erro ao carregar as questões.</p>";
        });
}

carregarQuestoes();

function excluirQuestao(idQuestao) {
    if (!idQuestao) return;

    if (confirm("Deseja excluir essa questão?")) {
        authFetch(`${API_BASE}/questoes/${idQuestao}`, {
            method: "DELETE"
        }).then(() => carregarQuestoes(paginaAtual));
    }
}
