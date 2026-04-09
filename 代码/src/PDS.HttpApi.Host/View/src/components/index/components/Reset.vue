<template>
  <a-modal :visible="true" title="修改密码" class="default-hb-modal" :width="600">
    <a-form-model layout="inline" :model="form" :rules="rules" ref="resetForm">
      <a-form-model-item label="旧密码" prop="currentPassword">
        <a-input-password v-model="form.currentPassword"></a-input-password>
      </a-form-model-item>
      <a-form-model-item label="新密码" prop="newPassword">
        <a-input-password v-model="form.newPassword"></a-input-password>
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small"> 取消 </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        保存
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import { Reset } from "@/api/account";
import component from "@/lib/base";
export default {
  name: "Reset",
  mixins: [component],
  data() {
    return {
      loading: false,
      rules: {
        currentPassword: [{ required: true, message: "请输入旧密码", trigger: "blur" }],
        newPassword: [{ required: true, message: "请输入新密码", trigger: "blur" }],
      },
      form: {
        currentPassword: "",
        newPassword: "",
      },
    };
  },
  methods: {
    save: async function () {
      this.loading = true;
      try {
        if (!(await this.confirm("确定修改密码?"))) return;
        if (!(await this.valid())) return;
        await Reset(this.form);
        this.$message.success("修改密码成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    close: function () {
      this.$emit("close");
    },
    valid: function () {
      return new Promise((resolve, reject) => {
        this.$refs.resetForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
