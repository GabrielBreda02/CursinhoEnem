// ===============================
// Script da Página de Cadastro/Edição de Tema de Redação
// ===============================

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const tituloInput = document.getElementById("titulo");
const anoInput = document.getElementById("ano");
const fonteInput = document.getElementById("fonte");
const textoMotivadorInput = document.getElementById("textoMotivador");

if (id) {
    document.getElementById("tituloPagina").textContent = "Editar Tema de Redação";
    fetch(`${API_BASE}/temas-redacao/${id}`)
        .then(res => res.json())
        .then(tema => {
            tituloInput.value = tema.titulo;
            anoInput.value = tema.ano || "";
            fonteInput.value = tema.fonte || "";
            textoMotivadorInput.value = tema.textoMotivador;
        });
}

function salvarTema() {
    const dados = {
        titulo: tituloInput.value.trim(),
        ano: anoInput.value ? Number(anoInput.value) : null,
        fonte: fonteInput.value.trim() || null,
        textoMotivador: textoMotivadorInput.value.trim()
    };

    if (!dados.titulo || !dados.textoMotivador) {
        alert("Preencha o título e o texto motivador.");
        return;
    }

    const metodo = id ? "PUT" : "POST";
    const url = id
        ? `${API_BASE}/temas-redacao/${id}`
        : `${API_BASE}/temas-redacao`;

    authFetch(url, {
        method: metodo,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dados)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro na requisição");
            }
            window.location.href = "TemasRedacao.html";
        })
        .catch(erro => {
            console.error("Erro ao salvar tema de redação:", erro);
            alert("Erro ao salvar tema de redação.");
        });
}
