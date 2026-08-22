// ===============================
// Script da Página de Resultado da Prova (aluno)
// ===============================

const params = new URLSearchParams(window.location.search);
const tentativaId = params.get("id");

const tituloProva = document.getElementById("tituloProva");
const placarNumero = document.getElementById("placarNumero");
const placarLegenda = document.getElementById("placarLegenda");
const questoesResultado = document.getElementById("questoesResultado");
const secaoRedacaoResultado = document.getElementById("secaoRedacaoResultado");
const temaRedacaoResultado = document.getElementById("temaRedacaoResultado");
const textoRedacaoResultado = document.getElementById("textoRedacaoResultado");
const notaRedacaoResultado = document.getElementById("notaRedacaoResultado");
const competenciasResultado = document.getElementById("competenciasResultado");
const comentarioRedacaoResultado = document.getElementById("comentarioRedacaoResultado");

const NOMES_COMPETENCIAS = [
    "C1 — Domínio da norma culta",
    "C2 — Compreensão do tema",
    "C3 — Argumentação",
    "C4 — Coesão textual",
    "C5 — Proposta de intervenção"
];

if (!tentativaId) {
    tituloProva.textContent = "Resultado não encontrado.";
} else {
    authFetch(`${API_BASE}/tentativas/${tentativaId}`)
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível carregar o resultado.");
            }
            return dados;
        })
        .then(renderizarResultado)
        .catch(erro => {
            tituloProva.textContent = "Erro ao carregar resultado";
            questoesResultado.innerHTML = `<p>${erro.message}</p>`;
        });
}

function renderizarResultado(dados) {
    tituloProva.textContent = dados.provaTitulo;
    placarNumero.textContent = `${dados.notaObjetivas} / ${dados.totalQuestoes}`;
    placarLegenda.textContent = "questões objetivas corretas";

    questoesResultado.innerHTML = "";
    dados.questoes.forEach((questao, index) => {
        const card = document.createElement("div");
        card.className = "question-card";

        const imagemHtml = questao.imagemUrl
            ? `<img src="${API_BASE.replace(/\/api$/, "")}${questao.imagemUrl}" style="max-width:100%;border-radius:8px;margin:8px 0;">`
            : "";

        const alternativasHtml = questao.alternativas.map(alt => {
            let classe = "";
            if (alt.correta) {
                classe = "alternativa-correta";
            } else if (alt.idAlternativa === questao.alternativaSelecionadaId) {
                classe = "alternativa-selecionada-errada";
            }
            const marcador = alt.idAlternativa === questao.alternativaSelecionadaId ? "→ " : "";
            return `<p class="${classe}">${marcador}${alt.descricao}</p>`;
        }).join("");

        const statusHtml = questao.respondidaCorretamente
            ? '<span class="status-correta">(Correta)</span>'
            : '<span class="status-incorreta">(Incorreta)</span>';

        card.innerHTML = `
            <h4>${index + 1}. ${questao.titulo} ${statusHtml}</h4>
            ${imagemHtml}
            ${alternativasHtml}
        `;

        questoesResultado.appendChild(card);
    });

    if (dados.temaRedacaoTitulo) {
        secaoRedacaoResultado.style.display = "block";
        temaRedacaoResultado.textContent = dados.temaRedacaoTitulo;
        textoRedacaoResultado.textContent = dados.textoRedacao || "(nenhum texto enviado)";

        if (dados.notaRedacao != null) {
            notaRedacaoResultado.textContent = `Nota da redação: ${dados.notaRedacao} / 1000`;
            notaRedacaoResultado.className = "status-correta";

            const notas = [dados.notaComp1, dados.notaComp2, dados.notaComp3, dados.notaComp4, dados.notaComp5];
            competenciasResultado.innerHTML = NOMES_COMPETENCIAS
                .map((nome, i) => `<p>${nome}: <strong>${notas[i]} / 200</strong></p>`)
                .join("");
            competenciasResultado.style.display = "block";
        } else {
            notaRedacaoResultado.textContent = "Aguardando correção do professor";
            notaRedacaoResultado.className = "";
        }

        if (dados.comentarioRedacao) {
            comentarioRedacaoResultado.style.display = "block";
            comentarioRedacaoResultado.innerHTML = `<strong>Comentário do professor:</strong><br>${dados.comentarioRedacao}`;
        }
    }
}
