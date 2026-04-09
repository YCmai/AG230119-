export default {
    namespaced: true,
    state: {
        user: null,
        culture: "zh-Hans",
        authed: null,
        version: ""
    },
    getters: {
        getCulture(state) {
            return state.culture;
        }
    },
    mutations: {
        setUser(state, payload) {
            state.user = payload;
        },
        setCulture(state, payload) {
            state.culture = payload;
        },
        setAuthed(state, payload) {
            state.authed = payload;
        },
        setVersion(state, payload) {
            state.version = payload;
        }
    }
}