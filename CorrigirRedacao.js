// ===============================
// Script da Página de Correção de Redação (professor)
// ===============================

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const alunoInfo = document.getElementById("alunoInfo");
const temaRedacaoTituloEl = document.getElementById("temaRedacaoTitulo");
const temaRedacaoTextoEl = document.getElementById("temaRedacaoTexto");
const textoRedacaoEl = document.getElementById("textoRedacao");
const comentarioRedacaoInput = document.getElementById("comentarioRedacao");
const notaTotalPreview = document.getElementById("notaTotalPreview");

const notaCompInputs = [1, 2, 3, 4, 5].map(n => document.getElementById(`notaComp${n}`));

function atualizarTotalPreview() {
    const valores = notaCompInputs.map(input => input.value);
    if (valores.some(v => v === "")) {
        notaTotalPreview.textContent = "";
        return;
    }
    const total = valores.reduce((soma, v) => soma + Number(v), 0);
    notaTotalPreview.textContent = `Total: ${total} / 1000`;
}

notaCompInputs.forEach(input => input.addEventListener("input", atualizarTotalPreview));

if (!id) {
    alunoInfo.textContent = "Redação não encontrada.";
} else {
    authFetch(`${API_BASE}/redacoes/${id}`)
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível carregar a redação.");
            }
            return dados;
        })
        .then(preencherTela)
        .catch(erro => {
            alunoInfo.textContent = erro.message;
        });
}

function preencherTela(dados) {
    alunoInfo.textContent = `${dados.alunoNome} — ${dados.provaTitulo}`;
    temaRedacaoTituloEl.textContent = dados.temaRedacaoTitulo;
    temaRedacaoTextoEl.textContent = dados.temaRedacaoTexto;
    textoRedacaoEl.textContent = dados.textoRedacao || "(o aluno não escreveu nada)";

    const notas = [dados.notaComp1, dados.notaComp2, dados.notaComp3, dados.notaComp4, dados.notaComp5];
    notaCompInputs.forEach((input, i) => input.value = notas[i] ?? "");
    atualizarTotalPreview();

    comentarioRedacaoInput.value = dados.comentarioRedacao || "";
}

function salvarCorrecao() {
    const notas = notaCompInputs.map(input => input.value);

    if (notas.some(v => v === "")) {
        alert("Preencha as 5 competências antes de salvar.");
        return;
    }

    const notasNumericas = notas.map(Number);
    if (notasNumericas.some(n => Number.isNaN(n) || n < 0 || n > 200 || n % 20 !== 0)) {
        alert("Cada competência vale de 0 a 200, em múltiplos de 20 (0, 20, 40 ... 200).");
        return;
    }

    authFetch(`${API_BASE}/redacoes/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            notaComp1: notasNumericas[0],
            notaComp2: notasNumericas[1],
            notaComp3: notasNumericas[2],
            notaComp4: notasNumericas[3],
            notaComp5: notasNumericas[4],
            comentarioRedacao: comentarioRedacaoInput.value.trim() || null
        })
    })
        .then(async response => {
            if (!response.ok) {
                const dados = await response.json().catch(() => ({}));
                throw new Error(dados.message || "Erro ao salvar a correção");
            }
            window.location.href = "Redacoes.html";
        })
        .catch(erro => {
            console.error(erro);
            alert(erro.message);
        });
}
