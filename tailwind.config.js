module.exports = {
    content: [
        './Pages/**/*.cshtml',
        './Views/**/*.cshtml',
        './Areas/**/*.cshtml'  /* <-- C'est cette ligne qui manquait ! */
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}