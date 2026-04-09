<template>
  <a-modal :visible="true" :width="620" title="修改库存可用数量" class="default-hb-modal">
    <a-form-model
      :model="form"
      ref="form"
      class="ant-form-lable-100"
      layout="inline"
      :rules="rules"
    >
      <a-form-model-item label="可用数量" prop="">
        <a-input v-model="form.newCount" :disabled="isEdit" />
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small">
        {{ $t("actions.cancel") }}
      </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        {{ $t("actions.save") }}
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import { InSearch, EditAvailableQuatity } from "@/api/stock/stock";
export default {
  name: "EditAvailableQuatityForm",
  data() {
    return {
      loading: false,
      form: {
        barCode: "",
        oldCount: "",
        newCount: "",
        wareHouseId: ""
      },
      rules: {
        sku: [{ required: true, message: "sku编码不能为空!", trigger: "blur" }],
        name: [{ required: true, message: "物料名称不能为空!", trigger: "blur" }],
      },
      isEdit: false,
    };
  },
  methods: {
    load: async function (input) {
      this.loading = true;
      try {
        this.form.oldCount = input.availableQuatity;
        this.form.newCount = input.availableQuatity;
        this.form = Object.assign(this.form,input);
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    save: async function () {
      if (!(await this.valid())) return;
      this.loading = true;
      try {
        await EditAvailableQuatity(this.form);
        this.$message.success("提交成功!");
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
      return new Promise((resolve) => {
        this.$refs.form.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
