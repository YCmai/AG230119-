import isGranted from "./permission";
export default {
    name: "base",
    methods: {
        confirm: function(content = '') {
            return new Promise((resolve, reject) => {
                this.$confirm({
                    title: "提示",
                    content: content || '',
                    okButtonProps: { props: { size: "small" } },
                    cancelButtonProps: { props: { size: "small" } },
                    class: "default-hb-confirm",
                    onOk: () => { resolve(true); },
                    onCancel: () => { resolve(false); },
                });
            });
        },
        isGranted: isGranted
    },
}