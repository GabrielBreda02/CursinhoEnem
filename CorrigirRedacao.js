// ===============================
// Script da Página de Correção de Redação (professor)
// ===============================

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const alunoInfo = document.getElementById("alunoInfo");
const temaRedacaoTituloEl = document.getElementById("temaRedacaoTitulo");
const temaRedacaoTextoEl = document.getElementById("temaRedacaoTexto");
const textoRedacaoEl = document.getElementById("textoRedacao");
const notaRedacaoInput = document.getElementById("notaRedacao");
const comentarioRedacaoInput = document.getElementById("comentarioRedacao");

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
    notaRedacaoInput.value = dados.notaRedacao ?? "";
    comentarioRedacaoInput.value = dados.comentarioRedacao || "";
}

function salvarCorrecao() {
    const nota = Number(notaRedacaoInput.value);

    if (notaRedacaoInput.value === "" || Number.isNaN(nota) || nota < 0 || nota > 1000) {
        alert("Informe uma nota entre 0 e 1000.");
        return;
    }

    authFetch(`${API_BASE}/redacoes/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            notaRedacao: nota,
            comentarioRedacao: comentarioRedacaoInput.value.trim() || null
        })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro ao salvar a correção");
            }
            window.location.href = "Redacoes.html";
        })
        .catch(erro => {
            console.error(erro);
            alert("Erro ao salvar a correção.");
        });
}
